import {AppTask, Aurelia, IContainer, ILogger, Registration} from "aurelia";
import {
    IActionProvider,
    IAppService,
    IBackgroundService,
    ICodeService,
    ICompletionItemProvider,
    IDataConnectionService,
    IPaneManager,
    IScriptService,
    IShortcutManager,
    IWindowBootstrapper,
    PaneHost,
    PaneManager,
    Settings,
    ShortcutManager,
} from "@application";
import {Window} from "./window";
import {Workbench} from "./workbench";
import {IStatusbarService, StatusbarService} from "./statusbar/statusbar-service";
import {IWorkAreaAppearance, WorkAreaAppearance} from "./work-area/work-area-appearance";
import {IWorkAreaService, WorkAreaService} from "./work-area/work-area-service";
import {ITextEditor, TextEditor} from "@application/editor/text-editor";
import {ITextEditorService} from "@application/editor/itext-editor-service";
import {TextEditorService} from "@application/editor/text-editor-service";
import {AppWindows} from "@application/windows/app-windows";
import {DialogUtil} from "@application/dialogs/dialog-util";
import {DialogBackgroundService} from "@application/background-services/dialog-background-service";
import {QuickTipsDialog} from "@application/app/quick-tips-dialog/quick-tips-dialog";
import {IMainMenuService} from "@application/main-menu/imain-menu-service";
import {MainMenuService} from "@application/main-menu/main-menu-service";
import {PaneToolbar} from "@application/panes/pane-toolbar";
import {CodeService} from "@application/code/code-service";
import {DataConnectionService} from "@application/data-connections/data-connection-service";
import {ScriptService} from "@application/scripts/script-service";
import {DataConnectionName} from "@application/data-connections/data-connection-name/data-connection-name";
import {BuiltinActionProvider} from "@application/editor/providers/builtin-action-provider";
import {BuiltinCSharpCompletionProvider} from "@application/editor/providers/builtin-csharp-completion-provider";
import {BuiltinSqlCompletionProvider} from "@application/editor/providers/builtin-sql-completion-provider";

export class MainWindowBootstrapper implements IWindowBootstrapper {
    constructor(private readonly logger: ILogger) {
    }

    public getEntry = () => Window;

    public registerServices(app: Aurelia): void {
        app.register(
            Registration.singleton(AppWindows, AppWindows),
            Registration.singleton(IScriptService, ScriptService),
            Registration.singleton(ITextEditorService, TextEditorService),
            Registration.singleton(ICodeService, CodeService),
            Registration.singleton(IDataConnectionService, DataConnectionService),
            Registration.singleton(IBackgroundService, DialogBackgroundService),
            Registration.singleton(IWorkAreaService, WorkAreaService),
            Registration.singleton(IMainMenuService, MainMenuService),
            Registration.singleton(IStatusbarService, StatusbarService),
            Registration.singleton(IWorkAreaAppearance, WorkAreaAppearance),
            Registration.singleton(Workbench, Workbench),
            Registration.transient(ITextEditor, TextEditor),
            Registration.singleton(IPaneManager, PaneManager),
            Registration.singleton(IShortcutManager, ShortcutManager),
            Registration.singleton(IActionProvider, BuiltinActionProvider),
            Registration.singleton(ICompletionItemProvider, BuiltinCSharpCompletionProvider),
            Registration.singleton(ICompletionItemProvider, BuiltinSqlCompletionProvider),
            PaneHost,
            PaneToolbar,
            DataConnectionName,

            // App startup task
            AppTask.activated(IContainer, async container => {
                const appService = container.get(IAppService);
                await appService.notifyClientAppIsReady();
                await QuickTipsDialog.showIfFirstVisit(container.get(DialogUtil));

                const settings = container.get(Settings);
                if (settings.autoCheckUpdates) {
                    appService.checkForUpdates();
                }
            })
        );

        try {
            this.registerPlugins(app.container);
        } catch (ex) {
            this.logger.error(`Error occurred while registering plugins`, ex);
        }
    }

    private registerPlugins(container: IContainer) {
        const requireContext = require.context('@plugins', true, /plugin\.ts$/);

        const pluginPaths = requireContext
            .keys()
            .filter((k: string) => {
                // For a plugin.ts file in "@plugins/plugin-dir/plugin.ts", require.context will return:
                // 1. "@plugins/plugin-dir/plugin.ts"
                // 2. "./plugin-dir/plugin.ts"
                // 3. "core/@plugins/plugin-dir/plugin.ts"
                //
                // We only want the one that starts with "@plugins"
                const startsWithPluginsRoot = k.startsWith("@plugins");

                // We don't want plugin.ts files that are not direct descendants of @plugins/plugin-dir/
                const directDescendantOfPluginDir = k
                    .replaceAll("\\", "/")
                    .split("/")
                    .length === 3;

                return startsWithPluginsRoot && directDescendantOfPluginDir;
            });

        for (const pluginPath of pluginPaths) {
            try {
                const plugin = requireContext(pluginPath);
                if (plugin.configure) {
                    plugin.configure(container);
                    this.logger.info(`Loaded plugin: ${pluginPath}`);
                }
            } catch (ex) {
                this.logger.error(`Could not load plugin: ${pluginPath}`, ex);
            }
        }
    }
}
