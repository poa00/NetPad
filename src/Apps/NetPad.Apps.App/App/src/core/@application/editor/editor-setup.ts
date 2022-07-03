import * as monaco from "monaco-editor";
import {all} from "aurelia";
import {
    ICompletionItemProvider,
    IDocumentRangeSemanticTokensProvider,
    IDocumentSemanticTokensProvider
} from "./interfaces";

export class EditorSetup {
    constructor(
        @all(ICompletionItemProvider) private readonly completionItemProviders: monaco.languages.CompletionItemProvider[],
        @all(IDocumentSemanticTokensProvider) private readonly documentSemanticTokensProviders: monaco.languages.DocumentSemanticTokensProvider[],
        @all(IDocumentRangeSemanticTokensProvider) private readonly documentRangeSemanticTokensProviders: monaco.languages.DocumentRangeSemanticTokensProvider[],
    ) {
    }

    public setup() {
        this.registerThemes();
        this.registerCompletionProviders();
        this.registerSemanticTokensProviders()
    }

    public static defineTheme(themeName: string, themeData: monaco.editor.IStandaloneThemeData) {
        if (!themeData.rules || !themeData.rules.length) {
            themeData.rules = themeData.base == "vs" ? this.lightThemeTokenThemeRules : this.darkThemeTokenThemeRules;
        }

        monaco.editor.defineTheme(themeName, themeData);
    }

    private registerThemes() {
        EditorSetup.defineTheme("netpad-light-theme", {
            base: "vs",
            inherit: true,
            rules: [],
            colors: {}
        });
        EditorSetup.defineTheme("netpad-dark-theme", {
            base: "vs-dark",
            inherit: true,
            rules: [],
            colors: {}
        });
    }

    private registerCompletionProviders() {
        for (const completionItemProvider of this.completionItemProviders) {
            monaco.languages.registerCompletionItemProvider("csharp", completionItemProvider);
        }
    }

    private registerSemanticTokensProviders() {
        for (const documentSemanticTokensProvider of this.documentSemanticTokensProviders) {
            monaco.languages.registerDocumentSemanticTokensProvider("csharp", documentSemanticTokensProvider);
        }

        for (const documentRangeSemanticTokensProvider of this.documentRangeSemanticTokensProviders) {
            monaco.languages.registerDocumentRangeSemanticTokensProvider("csharp", documentRangeSemanticTokensProvider);
        }
    }

    private static lightThemeTokenThemeRules: monaco.editor.ITokenThemeRule[] = [
        {token: "comment", foreground: "008000"},
        {token: "string", foreground: "a31515"},
        {token: "keyword", foreground: "0000ff"},
        {token: "number", foreground: "098658"},
        {token: "regexp", foreground: "EE0000"}, // same as string escape char
        {token: "operator", foreground: "000000"},
        {token: "namespace", foreground: "267f99"},
        {token: "type", foreground: "267f99"},
        {token: "struct", foreground: "267f99"},
        {token: "class", foreground: "267f99"},
        {token: "interface", foreground: "267f99"},
        {token: "enum", foreground: "267f99"},
        {token: "typeParameter", foreground: "267f99"},
        {token: "function", foreground: "795E26"},
        {token: "member", foreground: "795E26"},
        {token: "macro", foreground: "ffffff"},           // NEED
        {token: "variable", foreground: "001080"},
        {token: "parameter", foreground: "001080"},
        {token: "property", foreground: "001080"},
        {token: "enumMember", foreground: "0070C1"},
        {token: "event", foreground: "001080"},
        {token: "label", foreground: "ffffff"},           // NEED
        {token: "plainKeyword", foreground: "0000ff"},
        {token: "controlKeyword", foreground: "AF00DB"},
        {token: "operatorOverloaded", foreground: "ffffff"},  // NEED
        {token: "preprocessorKeyword", foreground: "0000ff"},
        {token: "preprocessorText", foreground: "a31515"},
        {token: "excludedCode", foreground: "ffffff"},        // NEED
        {token: "punctuation", foreground: "AF00DB"},         // same as control keyword
        {token: "stringVerbatim", foreground: "a31515"},
        {token: "stringEscapeCharacter", foreground: "EE0000"},
        {token: "delegate", foreground: "267f99"},
        {token: "module", foreground: "ffffff"},              // NEED
        {token: "extensionMethod", foreground: "795E26"},
        {token: "field", foreground: "001080"},
        {token: "local", foreground: "001080"},
        {token: "xmlDocCommentAttributeName", foreground: "008000"},
        {token: "xmlDocCommentAttributeQuotes", foreground: "008000"},
        {token: "xmlDocCommentAttributeValue", foreground: "008000"},
        {token: "xmlDocCommentCDataSection", foreground: "008000"},
        {token: "xmlDocCommentComment", foreground: "008000"},
        {token: "xmlDocCommentDelimiter", foreground: "008000"},
        {token: "xmlDocCommentEntityReference", foreground: "008000"},
        {token: "xmlDocCommentName", foreground: "008000"},
        {token: "xmlDocCommentProcessingInstruction", foreground: "008000"},
        {token: "xmlDocCommentText", foreground: "008000"},
        {token: "regexComment", foreground: "ffffff"},
        {token: "regexCharacterClass", foreground: "ffffff"},
        {token: "regexAnchor", foreground: "ffffff"},
        {token: "regexQuantifier", foreground: "ffffff"},
        {token: "regexGrouping", foreground: "ffffff"},
        {token: "regexAlternation", foreground: "ffffff"},
        {token: "regexSelfEscapedCharacter", foreground: "ffffff"},
        {token: "regexOtherEscape", foreground: "ffffff"},
    ]

    private static darkThemeTokenThemeRules: monaco.editor.ITokenThemeRule[] = [
        {token: "comment", foreground: "6A9955"},
        {token: "string", foreground: "ce9178"},
        {token: "keyword", foreground: "569cd6"},
        {token: "number", foreground: "b5cea8"},
        {token: "regexp", foreground: "D7BA7D"}, // same as string escape char
        {token: "operator", foreground: "d4d4d4"},
        {token: "namespace", foreground: "4EC9B0"},
        {token: "type", foreground: "4EC9B0"},
        {token: "struct", foreground: "4EC9B0"},
        {token: "class", foreground: "4EC9B0"},
        {token: "interface", foreground: "4EC9B0"},
        {token: "enum", foreground: "4EC9B0"},
        {token: "typeParameter", foreground: "4EC9B0"},
        {token: "function", foreground: "DCDCAA"},
        {token: "member", foreground: "DCDCAA"},
        {token: "macro", foreground: "ffffff"},           // NEED
        {token: "variable", foreground: "9CDCFE"},
        {token: "parameter", foreground: "9CDCFE"},
        {token: "property", foreground: "9CDCFE"},
        {token: "enumMember", foreground: "4FC1FF"},
        {token: "event", foreground: "9CDCFE"},
        {token: "label", foreground: "ffffff"},           // NEED
        {token: "plainKeyword", foreground: "569cd6"},
        {token: "controlKeyword", foreground: "C586C0"},
        {token: "operatorOverloaded", foreground: "ffffff"},  // NEED
        {token: "preprocessorKeyword", foreground: "569cd6"},
        {token: "preprocessorText", foreground: "ce9178"},
        {token: "excludedCode", foreground: "ffffff"},        // NEED
        {token: "punctuation", foreground: "C586C0"},         // same as control keyword
        {token: "stringVerbatim", foreground: "ce9178"},
        {token: "stringEscapeCharacter", foreground: "D7BA7D"},
        {token: "delegate", foreground: "4EC9B0"},
        {token: "module", foreground: "ffffff"},              // NEED
        {token: "extensionMethod", foreground: "DCDCAA"},
        {token: "field", foreground: "9CDCFE"},
        {token: "local", foreground: "9CDCFE"},
        {token: "xmlDocCommentAttributeName", foreground: "6A9955"},
        {token: "xmlDocCommentAttributeQuotes", foreground: "6A9955"},
        {token: "xmlDocCommentAttributeValue", foreground: "6A9955"},
        {token: "xmlDocCommentCDataSection", foreground: "6A9955"},
        {token: "xmlDocCommentComment", foreground: "6A9955"},
        {token: "xmlDocCommentDelimiter", foreground: "6A9955"},
        {token: "xmlDocCommentEntityReference", foreground: "6A9955"},
        {token: "xmlDocCommentName", foreground: "6A9955"},
        {token: "xmlDocCommentProcessingInstruction", foreground: "6A9955"},
        {token: "xmlDocCommentText", foreground: "6A9955"},
        {token: "regexComment", foreground: "ffffff"},
        {token: "regexCharacterClass", foreground: "ffffff"},
        {token: "regexAnchor", foreground: "ffffff"},
        {token: "regexQuantifier", foreground: "ffffff"},
        {token: "regexGrouping", foreground: "ffffff"},
        {token: "regexAlternation", foreground: "ffffff"},
        {token: "regexSelfEscapedCharacter", foreground: "ffffff"},
        {token: "regexOtherEscape", foreground: "ffffff"},
    ]
}