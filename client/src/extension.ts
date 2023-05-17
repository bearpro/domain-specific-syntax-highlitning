// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, SettingMonitor, ServerOptions, TransportKind, InitializeParams, Trace } from 'vscode-languageclient/node';

let client: LanguageClient

// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {
    console.log("activate")
    const serverExecutable = 'dotnet'
    const assemblyPath = 'C:/Users/bearp/source/personal/language-server-sample/server/bin/Debug/net6.0/langauge-server-sample.dll'
    const serverOptions: ServerOptions = {
        run: { command: serverExecutable, args: [assemblyPath] },
        debug: { command: serverExecutable, args: [assemblyPath] }
    }

    const clientOptions: LanguageClientOptions = {
        documentSelector: [ "**/*.smpl.dssh" ],
        synchronize: {
            // Synchronize the setting section 'languageServerExample' to the server
            configurationSection: "languageServerExample",
            fileEvents: vscode.workspace.createFileSystemWatcher("**/*.smpl.dssh"),
        },
    }

    client = new LanguageClient("languageServerExample", "Language Server Example", serverOptions, clientOptions)
    client.registerProposedFeatures()
    client.setTrace(Trace.Verbose)
    client.start()
    let x = 0
}

// This method is called when your extension is deactivated
export function deactivate() {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
