import * as vscode 
    from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, Trace } 
    from 'vscode-languageclient/node';

let client: LanguageClient

export function activate(context: vscode.ExtensionContext) {
    activateLanguageClient();

    const commandId = 'client.setup-config'
    const commandHandler = () => {
        const wsedit = new vscode.WorkspaceEdit();
        if (vscode.workspace.workspaceFolders === undefined)
        {
            vscode.window.showInformationMessage('No folders in workspace');
            return;
        }
        const folders = vscode.workspace.workspaceFolders ?? []
        const wsPath = folders[0].uri.fsPath;
        const filePath = vscode.Uri.file(wsPath + '/domains.dsh.conf');
        wsedit.createFile(filePath, { ignoreIfExists: true, contents: Buffer.from("abc\n    xyz\n    lol-kek") });
        vscode.workspace.applyEdit(wsedit);
        vscode.window.showInformationMessage(`Configuration file created (${filePath})`);
    }
    context.subscriptions.push(
        vscode.commands.registerCommand(commandId, commandHandler)
    )
}

function activateLanguageClient() {
    const serverExecutable = 'dotnet';
    const assemblyPath = '/home/bearpro/Source/repos/language-server-sample/server/bin/Debug/net6.0/langauge-server-sample.dll';
    const serverOptions: ServerOptions = {
        run: { command: serverExecutable, args: [assemblyPath] },
        debug: { command: serverExecutable, args: [assemblyPath] }
    };

    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            "**/*.dsh",
            "**/*.dsh.conf",
        ],
        synchronize: {
            configurationSection: "languageServerExample",
            fileEvents: [
                vscode.workspace.createFileSystemWatcher("**/*.dsh"),
                vscode.workspace.createFileSystemWatcher("**/*.dsh.conf"),
            ]
        },
    };

    client = new LanguageClient("languageServerExample", "Language Server Example", serverOptions, clientOptions);
    client.registerProposedFeatures();
    client.setTrace(Trace.Verbose);
    client.start();
}

export function deactivate() {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
