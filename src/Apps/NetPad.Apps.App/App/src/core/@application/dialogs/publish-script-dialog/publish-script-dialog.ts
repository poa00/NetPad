import {Dialog} from "../dialog";
import {DotNetPublishOptions, IDotNetPublishOptions, Script} from "@domain";
import {observable} from "@aurelia/runtime";
import * as path from "path";

export interface IPublishScriptDialog {
    script: Script;
}

enum ExportType {
    Executable = "Executable",
    Assembly = "Assembly"
}

export class PublishScriptDialog extends Dialog<IPublishScriptDialog> {
    private exportType: ExportType = ExportType.Executable;
    public options: DotNetPublishOptions;

    @observable public browsedFileList: FileList;
    private browseInput: HTMLInputElement;

    private rids = [
        "linux-arm",
        "linux-arm64",
        "linux-bionic-arm64",
        "linux-musl-x64",
        "linux-musl-arm64",
        "linux-x64",
        "osx-arm64",
        "osx-x64",
        "win-arm64",
        "win-x64",
        "win-x86",
    ]; // TODO we should try to default to the one that corresponds to current machine


    constructor() {
        super();
        this.options = DotNetPublishOptions.fromJS(<Partial<IDotNetPublishOptions>>{
            outputKind: "ConsoleApplication",
            optimizationLevel: "Release",
            selfContained: false,
        });
    }

    public get script() {
        return this.input?.script;
    }

    public get deploymentMode() {
        return this.options.selfContained ? "self-contained" : "framework-dependent";
    }

    public set deploymentMode(value) {
        this.options.selfContained = value === "self-contained";
    }

    public get runtimeId() {
        return this.options.runtimeId;
    }

    public set runtimeId(value) {
        this.options.runtimeId = !value ? undefined : value;
        if (!value) {
            this.options.publishReadyToRun = false;
            this.options.publishSingleFile = false;
            this.options.publishTrimmed = false;
            this.options.embedNativeLibraries = false;
            this.options.embedPdbs = false;
        }
    }

    public get publishCmd(): string {
        const directoryPath = this.options.directoryPath?.indexOf(" ") >= 0
            ? `"${this.options.directoryPath}"`
            : this.options.directoryPath ?? "";

        const cmd = [
            "dotnet publish",
            `-c ${this.options.optimizationLevel}`,
            `-o ${directoryPath}`,
        ];

        if (this.runtimeId) {
            cmd.push("-r", this.runtimeId);
        }

        cmd.push(`--self-contained ${this.options.selfContained}`);

        if (this.options.publishReadyToRun) {
            cmd.push("-p:PublishReadyToRun=true");
        }

        if (this.options.publishTrimmed) {
            cmd.push("-p:PublishTrimmed=true");
        }

        if (this.options.publishSingleFile) {
            cmd.push("-p:PublishSingleFile=true");
        }

        if (this.options.embedNativeLibraries) {
            cmd.push("-p:IncludeNativeLibrariesForSelfExtract=true");
        }

        if (this.options.embedNativeLibraries) {
            cmd.push("-p:IncludeNativeLibrariesForSelfExtract=true");
        }

        if (this.options.embedPdbs) {
            cmd.push("-p:DebugType=embedded");
        }

        return cmd.join(" ");
    }

    public browsedFileListChanged(newValue: FileList) {
        if (!newValue || newValue.length === 0) {
            return;
        }

        const firstFile = newValue.item(0)!;

        this.options.directoryPath = path.dirname(firstFile.path);
        this.browseInput.value = "";
    }

    public async publish() {
        alert("Publish: " + this.publishCmd);
    }
}
