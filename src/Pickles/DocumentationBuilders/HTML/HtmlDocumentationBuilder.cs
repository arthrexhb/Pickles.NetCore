﻿//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HtmlDocumentationBuilder.cs" company="PicklesDoc">
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
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using NGenerics.DataStructures.Trees;
using NGenerics.Patterns.Visitor;
using NLog;
using PicklesDoc.Pickles.DirectoryCrawler;
using PicklesDoc.Pickles.Extensions;

namespace PicklesDoc.Pickles.DocumentationBuilders.HTML
{
    public class HtmlDocumentationBuilder : IDocumentationBuilder
    {
        private static readonly Logger Log = LogManager.GetLogger("TODO");// TODO MethodBase.GetCurrentMethod().DeclaringType.Name);

        private readonly IConfiguration configuration;
        private readonly HtmlDocumentFormatter htmlDocumentFormatter;
        private readonly HtmlResourceWriter htmlResourceWriter;

        private readonly IFileSystem fileSystem;

        public HtmlDocumentationBuilder(
            IConfiguration configuration,
            HtmlDocumentFormatter htmlDocumentFormatter,
            HtmlResourceWriter htmlResourceWriter,
            IFileSystem fileSystem)
        {
            this.configuration = configuration;
            this.htmlDocumentFormatter = htmlDocumentFormatter;
            this.htmlResourceWriter = htmlResourceWriter;
            this.fileSystem = fileSystem;
        }

        #region IDocumentationBuilder Members

        public void Build(GeneralTree<INode> features)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Writing HTML to {0}", this.configuration.OutputFolder.FullName);
            }

            this.htmlResourceWriter.WriteTo(this.configuration.OutputFolder.FullName);

            var actionVisitor = new ActionVisitor<INode>(node => this.VisitNodes(features, node));
            features?.AcceptVisitor(actionVisitor);
        }

        private void VisitNodes(GeneralTree<INode> features, INode node)
        {
            if (node.IsIndexMarkDownNode())
            {
                return;
            }

            // If path2 is 'rooted', then Path.Combine returns path2. See http://stackoverflow.com/q/53102/206297.
            // The root node has a RelativePathFromRoot of '/'.  On Unix, this counts as a rooted path, 
            // and we end up trying to write index.html to the root of the filesystem.  So trim the leading /.
            var unrootedRelativePathFromRoot = node.RelativePathFromRoot.TrimStart(Path.DirectorySeparatorChar);

            string nodePath = Path.Combine(this.configuration.OutputFolder.FullName, unrootedRelativePathFromRoot);
            string htmlFilePath;

            if (node.NodeType == NodeType.Content)
            {
                htmlFilePath = nodePath.Replace(this.fileSystem.Path.GetExtension(nodePath), ".html");
                this.WriteContentNode(features, node, htmlFilePath);
            }
            else if (node.NodeType == NodeType.Structure)
            {
                this.fileSystem.Directory.CreateDirectory(nodePath);

                htmlFilePath = this.fileSystem.Path.Combine(nodePath, "index.html");
                    
                this.WriteContentNode(features, node, htmlFilePath);
            }
            else
            {
                // copy file from source to output
                this.fileSystem.File.Copy(node.OriginalLocation.FullName, nodePath, overwrite: true);
            }
        }

        private void WriteContentNode(GeneralTree<INode> features, INode node, string htmlFilePath)
        {
            // TODO: is this correct way of creating the stream?
            using (var writer = new System.IO.StreamWriter(new FileStream(htmlFilePath, FileMode.Create, FileAccess.ReadWrite)))
            {
                XDocument document = this.htmlDocumentFormatter.Format(node, features, this.configuration.FeatureFolder);
                document.Save(writer);
                // TODO writer.Close();
            }
        }

        #endregion
    }
}
