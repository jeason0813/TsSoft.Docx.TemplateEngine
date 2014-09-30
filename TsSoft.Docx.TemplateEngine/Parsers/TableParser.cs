﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TsSoft.Docx.TemplateEngine.Parsers
{
    /// <summary>
    /// Parse table tag
    /// </summary>
    /// <author>Георгий Поликарпов</author>
    class TableParser : GeneralParser
    {
        public static readonly XNamespace WordMlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        public static readonly XName SdtName = WordMlNamespace + "sdt";
        public static readonly XName SdtPrName = WordMlNamespace + "sdtPr";
        public static readonly XName TagName = WordMlNamespace + "tag";
        public static readonly XName SdtContentName = WordMlNamespace + "sdtContent";
        public static readonly XName ValAttributeName = WordMlNamespace + "val";

        public Tags.TableTag Do(XElement startElement)
        {
            if (startElement == null)
            {
                throw new ArgumentNullException("Аргумент не может быть null");
            }

            var endTableTag = NextTagElements(startElement, "EndTable").FirstOrDefault();
            if (endTableTag == null || TagElementsBetween(startElement, endTableTag, "Table").Any())
            {
                throw new Exception("Отсутсвует закрывающий тег таблицы.");
            }

            var table = new Tags.TableTag();

            var itemsElement = TagElementsBetween(startElement, endTableTag, "Items").FirstOrDefault();
            if (itemsElement == null || itemsElement.Value == "")
            {
                throw new Exception("Отсутсвует источник данных для таблицы.");
            }
            table.ItemsSource = itemsElement.Value;

            var dynamicRowElement = TagElementsBetween(startElement, endTableTag, "DynamicRow").FirstOrDefault();
            if (dynamicRowElement != null)
            {
                table.DynamicRow = (dynamicRowElement.Value == "") ? 0 : int.Parse(dynamicRowElement.Value);
            }

            var contentElement = TagElementsBetween(startElement, endTableTag, "Content").FirstOrDefault();
            if (contentElement == null)
            {
                throw new Exception("Отсутсвует тег контента.");
            }
            var endContentElement = TagElementsBetween(contentElement, endTableTag, "EndContent").FirstOrDefault();
            if (endContentElement == null || TagElementsBetween(contentElement, endContentElement, "Content").Any())
            {
                throw new Exception("Отсутсвует закрывающий тег контента.");
            }
            var tableElement = contentElement.ElementsAfterSelf(WordMlNamespace + "tbl").Where(element => element.IsBefore(endContentElement)).FirstOrDefault();
            if (tableElement != null)
            {
                table.Table = tableElement;
            }

            return table;
        }

        private IEnumerable<XElement> NextTagElements(XElement startElement, string tagName)
        {
            return startElement.ElementsAfterSelf(SdtName).Where(element => element.Element(SdtPrName).Element(TagName).Attribute(ValAttributeName).Value == tagName);
        }

        private IEnumerable<XElement> TagElementsBetween(XElement startElement, XElement endElement, string tagName)
        {
            return NextTagElements(startElement, tagName).Where(element => element.IsBefore(endElement));
        }
    }
}
