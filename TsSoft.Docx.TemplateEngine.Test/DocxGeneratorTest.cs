﻿using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Xml.Linq;
using TsSoft.Docx.TemplateEngine.Tags;
using TsSoft.Docx.TemplateEngine.Tags.Processors;

namespace TsSoft.Docx.TemplateEngine.Test
{
    using System.Linq;
    using System.Xml.Serialization;

    using TsSoft.Commons.Utils;

    [TestClass]
    public class DocxGeneratorTest
    {
        private DocxGenerator docxGenerator;
        private Mock<DocxPackage> docxPackageMock;
        private Mock<DocxPackagePart> docxPackagePartMock;
        private Mock<ITagParser> parserMock;
        private Mock<AbstractProcessor> processorMock;
        private Mock<DataReader> stringDataReaderMock;
        private Mock<DataReader> entityDataReaderMock;
        private Mock<DataReader> documentDataReaderMock;

        private Stream templateStream;

        private Stream outputStream;

        private XElement root;


        [TestMethod]
        public void StubbedTestGenerateDocxString()
        {
            this.InitializeStubbedExecution();
            this.docxGenerator.GenerateDocx(this.templateStream, this.outputStream, "whatever");
            this.MakeAssertions(this.stringDataReaderMock);
        }

        [TestMethod]
        public void StubbedTestGenerateDocxXDocument()
        {
            this.InitializeStubbedExecution();
            this.docxGenerator.GenerateDocx(this.templateStream, this.outputStream, new XDocument());
            this.MakeAssertions(this.documentDataReaderMock);
        }

        [TestMethod]
        public void StubbedTestGenerateDocxEntity()
        {
            this.InitializeStubbedExecution();
            this.docxGenerator.GenerateDocx(this.templateStream, this.outputStream, new A());
            this.MakeAssertions(this.entityDataReaderMock);
        }

        [TestMethod]
        public void TestActualGeneration()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "DocxPackageTest.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            const string DynamicText = "My anaconda don't";
            generator.GenerateDocx(
                input,
                output,
                new SomeEntityWrapper
                    {
                        Test = new SomeEntity
                            {
                                Text = DynamicText
                            }
                    });
            var package = new DocxPackage(output);
            package.Load();

            XDocument documentPartXml = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPartXml.Descendants(WordMl.SdtName).Any());
            Assert.IsNotNull(documentPartXml.Descendants(WordMl.TextRunName).Single(e => e.Value == DynamicText));
        }       

        [TestMethod]
        public void TestActualGenerationDocxWithHtmlLatTags()
        {
            //var data = new XDocument();
            var xDoc = new XDocument();
            var xElement = new XElement("test", "value = &raquo");
            xDoc.Add(xElement);
            XDocumentType docType = new XDocumentType("names", null, null, @"<!ENTITY laquo ""&#171;""><!ENTITY raquo ""&#187;"">");
            xDoc.AddFirst(docType);
            

            
         
            Console.WriteLine(xDoc);
            /*string internalSubset = string.Empty;
            //var docType = new XDocumentType("names", null, null,)
            var htmlLatEntitiesMap = new Dictionary<string, int>();
            htmlLatEntitiesMap.Add("raquo", );*/


            //Console.WriteLine(x);

        }

        [TestMethod]
        public void TestActualGenerationRepeater()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "Repeater.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);
            var package = new DocxPackage(output);
            package.Load();
            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }

        [TestMethod]
        public void TestActualGenerationRepeaterInIf()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "RepeaterInIf.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "dataRepeaterInIf.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);
            var package = new DocxPackage(output);
            package.Load();

            Assert.IsFalse(package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml.Descendants(WordMl.SdtName).Any());
        }

        [TestMethod]
        public void TestActualGenerationDoubleIfAndText()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "IfText2.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);
            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }
        /* NOT REVELANT
        [TestMethod]
        public void TestActualGenerationItemsAfterEndContent()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "RepeaterItemsAfterEndContent.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "dataItemsAfterEndContent.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data
                );
            var package = new DocxPackage(output);
            package.Load();
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }
        */
        [TestMethod]
        public void TestActualGenerationIfWithParagraphs()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "if.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data
                );
            var package = new DocxPackage(output);
            package.Load();
            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }
      
        [TestMethod]
        public void TestActualGenerationStaticTextAfterTag()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "corruptedDocxx.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "DemoData2.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);

            var package = new DocxPackage(output);
            package.Load();
            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Console.WriteLine(documentPart);
            Assert.IsTrue(documentPart.Root.Descendants(WordMl.TableCellName).All(element => element.Elements().All(el => el.Name != WordMl.TextRunName)));
            
        }

        [TestMethod]
        public void TestActualGenerationItemRepeaterNested()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNestedDemo.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNestedDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });
            var package = new DocxPackage(output);
            package.Load();
            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));         
        }

        /*[TestMethod]
        public void TestActualTextGenerationHTMLLat1Content()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "HTMLLat1Content.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "HTMLLat1ContentData.xml");

            var streamReader = new StreamReader(dataStream);

            var data = streamReader.ReadToEnd();

            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });
            var package = new DocxPackage(output);
            package.Load();
            Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));

        }*/

        [TestMethod]
        public void TestActualXDocumentGenerationHTMLLat1Content()
        {
            const string htmlContentText = @"
                &lt;html&gt;
		        &lt;head /&gt;
		        &lt;body&gt;
		        &lt;p&gt; Text is  &laquo;gfdgfdg &lt;/p&gt;
		        &lt;/body&gt;
		        &lt;/html&gt;
            ";
            var input = AssemblyResourceHelper.GetResourceStream(this, "HTMLLat1Content.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();            

            var data = new XDocument();
            data.Add(
                    new XElement("test", new XElement(
                        "hypertext", htmlContentText 
                        )
                    )
            );

            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });
            var package = new DocxPackage(output);
            package.Load();

            using (var fileStream = File.Create("C:\\xdochtmlcontent.docx"))
            {
                output.Seek(0, SeekOrigin.Begin);
                output.CopyTo(fileStream);
            }

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));

        }

        [TestMethod]
        public void TestActualGenerationItemHtmlContentInTable()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemHtmlContentInTable.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.Ignore });
            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }

        [TestMethod]
        public void TestActualGenerationItemRepeater()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterDemo.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException});

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.ToString());
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());   
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));         
        }

        [TestMethod]
        public void TestActualGenerationItemRepeaterNestedTwoRepeaters()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemo.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            //Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }

        [TestMethod]
        public void TestActualGenerationRepeaterItemRepeaterItemTableItemTable()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "RepeaterSimplifiedDemo.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "DemoSampleData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            //Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));            
            Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }

        [TestMethod]
        public void TestAddAfterXelement()
        {
            XElement element = new XElement("element", new XElement("temp"));
            XElement afterelement = new XElement("afterelement");            
            var tmpDoc = new XDocument(element);              
            //tmpDoc.
            Console.WriteLine(tmpDoc);            
        }

        [TestMethod]
        public void TestActualGenerationItemRepeaterNestedTwoRepeatersWithoutSeparator()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoWithoutSeparator.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            //Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));

        }
        
        [TestMethod]
        public void TestActualGenerationItemTableInTable()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemTableDemo.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemTableDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            //Console.WriteLine(documentPart.Descendants(documentPart.Root.Elements(WordMl.TableRowName).Skip(2).Take(1).ToString()));
            Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants(WordMl.TableName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));            
        }


        [TestMethod]
        public void TestActualGenerationTemplate2()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "Template3.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemTableDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
           // Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            Console.WriteLine(documentPart);           
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }   

        [TestMethod]
        public void TestActualGenerationItemRepeaterNestedOneRepeaterWithEndItemRepeaterAndItemIndexInOneParagraph()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNestedDemo20.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            //Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }

        [TestMethod]
        public void TestActualGenerationItemHtmlContentInTble()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "badhtml.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "badhtml_data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            //Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }
        
        [TestMethod]
        public void TestActualGenerationItemRepeaterNestedDemoIRInParagraph()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNestedDemoIRParagraph.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            //Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
            Assert.IsFalse(documentPart.Descendants(WordMl.TableCellName).Elements(WordMl.ParagraphName).Any(el => el.Name.Equals(WordMl.TextRunName)));
        }

        [TestMethod]
        public void TestActualGenerationItemRepeaterNestedInOneParagraph()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNestedInOneParagraph.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            var firstRow =
                documentPart.Descendants(WordMl.TableRowName)
                       .First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1"));
            Console.WriteLine(firstRow);            
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());            
            Assert.IsFalse(firstRow.Elements().Last(el => el.Name.Equals(WordMl.TableCellName)).Elements().Any(el => el.Name.Equals(WordMl.TextRunName)));
        }     
               
        [TestMethod]
        public void TestActualGenerationItemRepeaterNestedTwoRepeatersWithStaticText()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoWithStaticText.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNested2IRDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            //Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }

        [TestMethod]
        public void TestActualGenerationItemRepeaterNestedElementsAfterEndItemRepeater()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNestedDemo20.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterNestedDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "Certificate 1")));
            //Console.WriteLine(documentPart);
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }


        [TestMethod]
        public void TestActualGenerationItemRepeaterElementsInParagraphs()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "nowbadplan_one.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "PlanDocx.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "тестовое ЭА мероприятие")));
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));         
        }        

        [TestMethod]
        public void TestActualGenerationItemRepeaterInRepeater()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "IRinRepeater.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ItemRepeaterDemoData.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException});

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.ToString());
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }

        [TestMethod]
        public void TestActualGenerationItemRepeaterElementsInParagraphs2()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "badplan.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "PlanDocx.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.Descendants(WordMl.TableRowName).First(tr => tr.Descendants().Any(el => el.Value == "тестовое ЭА мероприятие")));
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }

        [TestMethod]
        public void TestActualGenerationDoubleItemIfWithItemText()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "corruptedDoc.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "DemoData2.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, 
                new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException });

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.ToString());
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());                        
        }
        [TestMethod]
        public void TestActualGenerationTextInTable()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "textintable.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
           // documentPart.Descendants(WordMl.ParagraphName).Remove();
            Console.WriteLine(documentPart.Descendants(WordMl.TableName).First());
            Assert.IsTrue(documentPart.Descendants(WordMl.TableRowName).All(element => element.Elements().All(el => el.Name != WordMl.ParagraphName)));

        }
        [TestMethod]
        public void TestActualGenerationIfWithTableWithParagraphs()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ifTtable1.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }

        [TestMethod]
        public void TestActualGenerationRepeaterWithTextWithParagraphs()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "repeatertext1.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }

        [TestMethod]
        public void TestActualGenerationDoubleIf()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "DoubleIf.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }

        [TestMethod]
        public void TestActualGenerationIfInline()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "ifinline.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "ifinline_data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }
 
        [TestMethod]
        public void TestActualGenerationDoubleRepeater()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "DoubleRepeater.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data);

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
        }

        [TestMethod]
        public void TestActualGenerationDoubleHtmlContent()
        {
            var input = AssemblyResourceHelper.GetResourceStream(this, "DoubleHtmlContent.docx");
            var output = new MemoryStream();
            var generator = new DocxGenerator();
            var dataStream = AssemblyResourceHelper.GetResourceStream(this, "data.xml");
            var data = XDocument.Load(dataStream);
            generator.GenerateDocx(
                input,
                output,
                data, new DocxGeneratorSettings() { MissingDataMode = MissingDataMode.ThrowException});

            var package = new DocxPackage(output);
            package.Load();

            var documentPart = package.Parts.Single(p => p.PartUri.OriginalString.Contains("document.xml")).PartXml;
            Console.WriteLine(documentPart.ToString());
            Assert.IsFalse(documentPart.Descendants(WordMl.SdtName).Any());
            Assert.IsFalse(documentPart.Descendants(WordMl.ParagraphName).Descendants().Any(el => el.Name == WordMl.ParagraphName));
        }
        private void InitializeStubbedExecution()
        {
            this.docxPackageMock = new Mock<DocxPackage>();
            this.docxPackageMock.Setup(p => p.Load()).Verifiable();
            this.docxPackageMock.Setup(p => p.Save()).Verifiable();

            this.docxPackagePartMock = new Mock<DocxPackagePart>();
            var document = new XDocument();
            this.root = document.Root;

            this.docxPackagePartMock.SetupGet(p => p.PartXml).Returns(document);
            this.docxPackagePartMock.Setup(p => p.Save()).Verifiable();

            var packageParts = new List<DocxPackagePart> { this.docxPackagePartMock.Object };
            this.docxPackageMock.SetupGet(x => x.Parts).Returns(packageParts);

            var packageFactoryMock = new Mock<IDocxPackageFactory>();
            packageFactoryMock.Setup(f => f.Create(It.IsAny<Stream>())).Returns(this.docxPackageMock.Object);

            this.processorMock = new Mock<AbstractProcessor>();
            this.processorMock.Setup(p => p.Process()).Verifiable();
            this.processorMock.SetupSet(processor => processor.DataReader = It.IsAny<DataReader>()).Verifiable();
            var processorFactoryMock = new Mock<IProcessorFactory>();
            processorFactoryMock.Setup(f => f.Create()).Returns(this.processorMock.Object);

            this.parserMock = new Mock<ITagParser>();
            var parserFactoryMock = new Mock<IParserFactory>();
            parserFactoryMock.Setup(f => f.Create()).Returns(this.parserMock.Object);

            this.stringDataReaderMock = new Mock<DataReader>();
            this.entityDataReaderMock = new Mock<DataReader>();
            this.documentDataReaderMock = new Mock<DataReader>();
            var dataReaderFactoryMock = new Mock<IDataReaderFactory>();
            dataReaderFactoryMock.Setup(f => f.CreateReader(It.IsAny<string>())).Returns(this.stringDataReaderMock.Object);
            dataReaderFactoryMock.Setup(f => f.CreateReader(It.IsAny<A>())).Returns(this.entityDataReaderMock.Object);
            dataReaderFactoryMock.Setup(f => f.CreateReader(It.IsAny<XDocument>())).Returns(this.documentDataReaderMock.Object);

            this.docxGenerator = new DocxGenerator
            {
                DataReaderFactory = dataReaderFactoryMock.Object,
                PackageFactory = packageFactoryMock.Object,
                ParserFactory = parserFactoryMock.Object,
                ProcessorFactory = processorFactoryMock.Object
            };

            this.templateStream = AssemblyResourceHelper.GetResourceStream(this, "DocxPackageTest.docx");
            this.outputStream = new MemoryStream();
        }

        private void MakeAssertions(IMock<DataReader> dataReaderMock)
        {
            this.docxPackageMock.Verify(p => p.Load(), Times.Once);

            this.parserMock.Verify(p => p.Parse(It.IsAny<AbstractProcessor>(), It.IsAny<XElement>()), Times.Once);
            this.parserMock.Verify(p => p.Parse(this.processorMock.Object, this.root), Times.Once);

            this.processorMock.VerifySet(p => p.DataReader = It.IsAny<DataReader>(), Times.Once);
            this.processorMock.VerifySet(p => p.DataReader = dataReaderMock.Object, Times.Once);
            this.processorMock.Verify(p => p.Process(), Times.Once);

            this.docxPackageMock.Verify(p => p.Save(), Times.Once);
        }

        public class SomeEntity
        {
            [XmlElement("text")]
            public string Text { get; set; }
        }

        public class SomeEntityWrapper
        {
            [XmlElement("test")]
            public SomeEntity Test { get; set; }
        }

        internal class A
        {
            public int MyProperty { get; set; }
        }
    }
}