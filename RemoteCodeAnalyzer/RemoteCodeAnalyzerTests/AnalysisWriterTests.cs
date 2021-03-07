using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RemoteCodeAnalyzerTests
{
    [TestClass]
    class AnalysisWriterTests
    {
        [TestMethod]
        public void TestFileEquality()
        {
            // Expected
            XDocument oldFunctionAnalysis = XDocument.Load("D:\\CSE-681_Project3\\Root\\testuser2\\2021-02-24_PipelineHazardDetector\\2021-02-24_PipelineHazardDetector_functions.cs_old.xml");
            XDocument oldRelationshipAnalysis = XDocument.Load("D:\\CSE-681_Project3\\Root\\testuser2\\2021-02-24_PipelineHazardDetector\\2021-02-24_PipelineHazardDetector_relationships.cs_old.xml");

            // Actual
            XDocument newFunctionAnalysis = XDocument.Load("D:\\CSE-681_Project3\\Root\\testuser2\\2021-02-24_PipelineHazardDetector\\2021-02-24_PipelineHazardDetector_functions.cs.xml");
            XDocument newRelationshipAnalysis = XDocument.Load("D:\\CSE-681_Project3\\Root\\testuser2\\2021-02-24_PipelineHazardDetector\\2021-02-24_PipelineHazardDetector_relationships.cs.xml");

            TestXObjectEquality(oldFunctionAnalysis, newFunctionAnalysis);
            TestXObjectEquality(oldRelationshipAnalysis, newRelationshipAnalysis);
        }

        private void TestXObjectEquality(XObject xExpected, XObject xActual)
        {
            Assert.AreEqual(xExpected.GetType(), xActual.GetType());
            Assert.AreEqual(xExpected.NodeType, xActual.NodeType);

            if (xExpected.GetType() == typeof(XDocument))
            {
                TestXObjectEquality(((XDocument)xExpected).Root, ((XDocument)xActual).Root);
            }
            else if (xExpected.GetType() == typeof(XElement))
            {
                Assert.AreEqual(((XElement)xExpected).Name, ((XElement)xActual).Name);

                if (((XElement)xExpected).Name.ToString().Equals("file") || ((XElement)xExpected).Name.ToString().Equals("namespace")
                    || ((XElement)xExpected).Name.ToString().Equals("interface") || ((XElement)xExpected).Name.ToString().Equals("class")
                    || ((XElement)xExpected).Name.ToString().Equals("function"))
                {
                    Assert.AreEqual(((XElement)xExpected).Attribute("name").Value, ((XElement)xActual).Attribute("name").Value);
                }
                else if (!((XElement)xExpected).Name.ToString().Equals("project"))
                {
                    Assert.AreEqual(((XElement)xExpected).Value, ((XElement)xActual).Value);
                }

                IEnumerable<XElement> ixExpectedChildren = ((XElement)xExpected).Elements();
                IEnumerable<XElement> ixActualChildren = ((XElement)xActual).Elements();
                List<XElement> xExpectedChildren = new List<XElement>();
                List<XElement> xActualChildren = new List<XElement>();
                xExpectedChildren.AddRange(ixExpectedChildren);
                xActualChildren.AddRange(ixActualChildren);

                Assert.AreEqual(xExpectedChildren.Count, xActualChildren.Count);

                for (int i = 0; i < xExpectedChildren.Count; i++)
                {
                    TestXObjectEquality(xExpectedChildren[i], xActualChildren[i]);
                }
            }
        }
    }
}
