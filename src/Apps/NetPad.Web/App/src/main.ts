import Aurelia, {Registration} from 'aurelia';
// import "bootstrap";
// import 'bootstrap/dist/js/bootstrap.bundle';
import './styles/main.scss';
import 'bootstrap-icons/font/bootstrap-icons.scss';
import {Index} from './main-window';
import {IQueryManager, QueryManager, ISession, Session} from "@domain";

Aurelia
    .register(
        Registration.instance(String, "http://localhost:8001"),
        Registration.singleton(ISession, Session),
        Registration.singleton(IQueryManager, QueryManager),
    )
    .app({
        host: document.getElementsByTagName("main-window")[0],
        component: Index
    })
    .start();
