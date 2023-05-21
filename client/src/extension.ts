import * as vscode 
    from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, Trace } 
    from 'vscode-languageclient/node';

let client: LanguageClient

export function activate(context: vscode.ExtensionContext) {
    const serverExecutable = 'dotnet'
    const assemblyPath = '<путь к dll языкового сервера>'
    const serverOptions: ServerOptions = {
        run: { command: serverExecutable, args: [assemblyPath] },
        debug: { command: serverExecutable, args: [assemblyPath] }
    }

    const clientOptions: LanguageClientOptions = {
        documentSelector: [ "**/*.smpl.dssh" ],
        synchronize: {
            configurationSection: "languageServerExample",
            fileEvents: vscode.workspace.createFileSystemWatcher("**/*.smpl.dssh"),
        },
    }

    client = new LanguageClient("languageServerExample", "Language Server Example", serverOptions, clientOptions)
    client.registerProposedFeatures()
    client.setTrace(Trace.Verbose)
    client.start()
}

export function deactivate() {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
