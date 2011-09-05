using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace NntpClient.Nzb {
    /// <summary>
    /// Represents a posting on usenet.
    /// </summary>
    public class NzbDocument {
        /// <summary>
        /// Loads an nzb from a file path or Url &amp; validates it against the DTD.
        /// </summary>
        /// <param name="Uri">path to the nzb document</param>
        public NzbDocument(string Uri) {
            var validDoc = Validate(XDocument.Load(Uri));
            Parse(validDoc);
        }

        private XDocument Validate(XDocument nzbDoc) {
            var settings = new XmlReaderSettings {
                ValidationType = ValidationType.DTD,
                DtdProcessing = DtdProcessing.Parse
            };
            XmlReader r = XmlReader.Create(new StringReader(nzbDoc.ToString()), settings);

            return XDocument.Load(r);
        }
        private void Parse(XDocument nzb) {
            var ns = nzb.Root.GetDefaultNamespace();

            if(nzb.Root.Element("head") != null) {
                Metadata = nzb.Root.Element("head")
                    .Elements("meta")
                    .ToDictionary(k => k.Attribute("type").Value, v => v.Value);
            } else {
                Metadata = new Dictionary<string, string>();
            }

            Files = nzb.Root.Elements(ns + "file")
                .Select(e => new NzbFile(e, ns)).AsEnumerable();
        }

        /// <summary>
        /// Gets any metadata contained in the nzb
        /// </summary>
        public Dictionary<string, string> Metadata { get; private set; }
        /// <summary>
        /// Gets the collection of files in the nzb
        /// </summary>
        public IEnumerable<NzbFile> Files { get; private set; }
    }
}
