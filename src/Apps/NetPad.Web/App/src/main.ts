import Aurelia, {Registration} from 'aurelia';
// import "bootstrap";
// import 'bootstrap/dist/js/bootstrap.bundle';
import './styles/main.scss';
import 'bootstrap-icons/font/bootstrap-icons.scss';

const startupOptions = new URLSearchParams(window.location.search);
const win = startupOptions.get("win");

const app = Aurelia.register(
    Registration.instance(URLSearchParams, startupOptions),
    Registration.instance(String, window.location.origin),
);

if (win === "main") {
    const mainWindow = require("./windows/main/main");
    mainWindow.register(app);
} else if (win === "settings") {
    const settingsWindow = require("./windows/settings/main");
    settingsWindow.register(app);
} else if (win === "script-config") {
    const scriptConfigWindow = require("./windows/script-config/main");
    scriptConfigWindow.register(app);
}

app.start();