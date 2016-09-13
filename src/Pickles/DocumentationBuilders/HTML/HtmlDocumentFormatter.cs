//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HtmlDocumentFormatter.cs" company="PicklesDoc">
//  Copyright 2011 Jeffrey Cameron
//  Copyright 2012-present PicklesDoc team and community contributors
//
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Abstractions;
using System.Xml.Linq;
using NGenerics.DataStructures.Trees;
using PicklesDoc.Pickles.DirectoryCrawler;
using PicklesDoc.Pickles.Extensions;

namespace PicklesDoc.Pickles.DocumentationBuilders.HTML
{
    public class HtmlDocumentFormatter
    {
        private const string DocumentReady =
            "\n" +
            "$(document).ready(function() {" + "\n" +
            "  initializeToc();" + "\n" +
            "});" + "\n";

        private readonly IConfiguration configuration;
        private readonly HtmlContentFormatter htmlContentFormatter;
        private readonly HtmlFooterFormatter htmlFooterFormatter;
        private readonly HtmlHeaderFormatter htmlHeaderFormatter;
        private readonly HtmlResourceSet htmlResources;
        private readonly IFileSystem fileSystem;
        private readonly HtmlTableOfContentsFormatter htmlTableOfContentsFormatter;

        public HtmlDocumentFormatter(
            IConfiguration configuration,
            HtmlHeaderFormatter htmlHeaderFormatter,
            HtmlTableOfContentsFormatter htmlTableOfContentsFormatter,
            HtmlContentFormatter htmlContentFormatter,
            HtmlFooterFormatter htmlFooterFormatter,
            HtmlResourceSet htmlResources,
            IFileSystem fileSystem)
        {
            this.configuration = configuration;
            this.htmlHeaderFormatter = htmlHeaderFormatter;
            this.htmlTableOfContentsFormatter = htmlTableOfContentsFormatter;
            this.htmlContentFormatter = htmlContentFormatter;
            this.htmlFooterFormatter = htmlFooterFormatter;
            this.htmlResources = htmlResources;
            this.fileSystem = fileSystem;
        }

        public XDocument Format(INode featureNode, GeneralTree<INode> features, DirectoryInfoBase rootFolder)
        {
            XNamespace xmlns = HtmlNamespace.Xhtml;

            // If path2 is 'rooted', then Path.Combine returns path2. See http://stackoverflow.com/q/53102/206297.
            // The root node has a RelativePathFromRoot of '/'.  On Unix, this counts as a rooted path, 
            // and we end up trying to write index.html to the root of the filesystem.  So trim the leading /.
            var unrootedRelativePathFromRoot = featureNode.RelativePathFromRoot.TrimStart(Path.DirectorySeparatorChar);

            string featureNodeOutputPath = this.fileSystem.Path.Combine(
                this.configuration.OutputFolder.FullName,
                unrootedRelativePathFromRoot);

            // If it was the root node, Path.Combine will now have lost the directory separator, which
            // marks it as a directory.  We need this is order for MakeRelativeUri to work correctly.
            if (string.IsNullOrWhiteSpace(unrootedRelativePathFromRoot))
                featureNodeOutputPath += Path.DirectorySeparatorChar;

            var featureNodeOutputUri = UriUtility.CreateSourceUri(featureNodeOutputPath);

            var container = new XElement(xmlns + "div", new XAttribute("id", "container"));
            container.Add(this.htmlHeaderFormatter.Format());
            container.Add(this.htmlTableOfContentsFormatter.Format(featureNode.OriginalLocationUrl, features, rootFolder));
            container.Add(this.htmlContentFormatter.Format(featureNode, features));
            container.Add(this.htmlFooterFormatter.Format());

            var body = new XElement(xmlns + "body");
            body.Add(container);

            var head = new XElement(xmlns + "head");
            head.Add(new XElement(xmlns + "title", featureNode.Name));

            head.Add(
                new XElement(
                    xmlns + "link",
                    new XAttribute("rel", "stylesheet"),
                    new XAttribute(
                        "href",
                        featureNodeOutputUri.MakeRelativeUri(this.htmlResources.MasterStylesheet)),
                    new XAttribute("type", "text/css")));

            head.Add(
                new XElement(
                    xmlns + "link",
                    new XAttribute("rel", "stylesheet"),
                    new XAttribute(
                        "href",
                        featureNodeOutputUri.MakeRelativeUri(this.htmlResources.PrintStylesheet)),
                    new XAttribute("type", "text/css"),
                    new XAttribute("media", "print")));

            head.Add(
                new XElement(
                    xmlns + "script",
                    new XAttribute("src", featureNodeOutputUri.MakeRelativeUri(this.htmlResources.JQueryScript)),
                    new XAttribute("type", "text/javascript"),
                    new XText(string.Empty)));

            head.Add(
                new XElement(
                    xmlns + "script",
                    new XAttribute(
                        "src",
                        featureNodeOutputUri.MakeRelativeUri(this.htmlResources.AdditionalScripts)),
                    new XAttribute("type", "text/javascript"),
                    new XText(string.Empty)));

            head.Add(
                new XElement(
                    xmlns + "script",
                    new XAttribute("type", "text/javascript"),
                    DocumentReady));

            head.Add(new XComment(" We are using Font Awesome - http://fortawesome.github.com/Font-Awesome - licensed under CC BY 3.0 "));

            var html = new XElement(
                xmlns + "html",
                new XAttribute(XNamespace.Xml + "lang", "en"),
                head,
                body);

            var document = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XDocumentType(
                    "html",
                    "-//W3C//DTD XHTML 1.0 Strict//EN",
                    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd",
                    string.Empty),
                html);

            return document;
        }
    }
}
