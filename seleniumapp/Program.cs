using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace FacebookAutomation
{
    class Program
    {
      
        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var testCases = GenerateTestCases(1000);
            var options = new ChromeOptions();
            options.AddArguments("--headless"); 
            using var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("file:///C:/Users/sriha/Downloads/Simple%20Calculator.html");
            var results = RunTests(driver, testCases);
            ExportResultsToExcel(results);
        }

        static List<TestCase> GenerateTestCases(int numberOfCases)
        {
            var rand = new Random();
            var testCases = new List<TestCase>();
            var operations = new[] { "+", "-", "*", "/" };

            for (int i = 0; i < numberOfCases; i++)
            {
                var firstOperand = rand.Next(5000, 10000);
                var secondOperand = rand.Next(5000, 10000);
                var operation = operations[rand.Next(operations.Length)];

                var expectedResult = operation switch
                {
                    "+" => (firstOperand + secondOperand).ToString(),
                    "-" => (firstOperand - secondOperand).ToString(),
                    "*" => (firstOperand * secondOperand).ToString(),
                    "/" => secondOperand == 0 ? "Infinity" : (firstOperand / (double)secondOperand).ToString("0.#####"),
                    _ => "0"
                };

                testCases.Add(new TestCase
                {
                    FirstOperand = firstOperand.ToString(),
                    SecondOperand = secondOperand.ToString(),
                    Operation = operation,
                    ExpectedResult = expectedResult
                });
            }

            return testCases;
        }

        static List<TestResult> RunTests(IWebDriver driver, List<TestCase> testCases)
        {
            var results = new List<TestResult>();
            foreach (var testCase in testCases)
            {
                driver.FindElement(By.ClassName("all-clear")).Click();
                foreach (var ch in testCase.FirstOperand)
                {
                    driver.FindElement(By.Id($"btn{ch}")).Click();
                }
                driver.FindElement(By.Id(testCase.Operation switch
                {
                    "+" => "add",
                    "-" => "sub",
                    "*" => "mul",
                    "/" => "div",
                    _ => throw new InvalidOperationException()
                })).Click();
                foreach (var ch in testCase.SecondOperand)
                {
                    driver.FindElement(By.Id($"btn{ch}")).Click();
                }

                driver.FindElement(By.ClassName("equal-sign")).Click();
                var actualResult = driver.FindElement(By.Id("screen")).GetAttribute("value");
                  results.Add(new TestResult
                {
                    TestCase = testCase,
                    ActualResult = actualResult,
                    IsPass = actualResult == testCase.ExpectedResult
                });
            }
            return results;
        }

        static void ExportResultsToExcel(List<TestResult> results)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Test Results");
            worksheet.Cells[1, 1].Value = "Number 1";
            worksheet.Cells[1, 2].Value = "Operation";
            worksheet.Cells[1, 3].Value = "Number 2";
            worksheet.Cells[1, 4].Value = "Expected Output";
            worksheet.Cells[1, 5].Value = "Actual output";
            worksheet.Cells[1, 6].Value = "Output";
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                worksheet.Cells[i + 2, 1].Value = result.TestCase.FirstOperand;
                worksheet.Cells[i + 2, 2].Value = result.TestCase.Operation;
                worksheet.Cells[i + 2, 3].Value = result.TestCase.SecondOperand;
                worksheet.Cells[i + 2, 4].Value = result.TestCase.ExpectedResult;
                worksheet.Cells[i + 2, 5].Value = result.ActualResult;
                worksheet.Cells[i + 2, 6].Value = result.IsPass ? "pass" : "fail";
            }
            var fileInfo = new FileInfo("TestOutput.xlsx");
            package.SaveAs(fileInfo);
        }
    }

    class TestCase
    {
        public string FirstOperand { get; set; }
        public string SecondOperand { get; set; }
        public string Operation { get; set; }
        public string ExpectedResult { get; set; }
    }

    class TestResult
    {
        public TestCase TestCase { get; set; }
        public string ActualResult { get; set; }
        public bool IsPass { get; set; }
    }

}
