using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Tools;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;

namespace filescrubberMCP.Tests.Tools;

public class ParserToolsTests
{
    private readonly Mock<ILogger<ParserTools>> _mockLogger;
    private readonly Mock<IParserService> _mockParserService;
    private readonly ParserTools _parserTools;

    public ParserToolsTests()
    {
        _mockLogger = new Mock<ILogger<ParserTools>>();
        _mockParserService = new Mock<IParserService>();
        _parserTools = new ParserTools(_mockLogger.Object, _mockParserService.Object);
    }

    [Fact]
    public async Task SearchJson_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = "test.json";
        var jsonPath = "$.data";
        var expectedResult = "[{\"name\":\"item1\"}]";
        _mockParserService.Setup(x => x.SearchJsonFile(filePath, jsonPath, true, false))
            .Returns(expectedResult);

        // Act
        var result = await _parserTools.SearchJson(filePath, jsonPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(filePath, result);
        Assert.Contains(jsonPath, result);
        _mockParserService.Verify(x => x.SearchJsonFile(filePath, jsonPath, true, false), Times.Once);
    }

    [Fact]
    public async Task SearchJson_WithShowKeyPaths_PassesParameter()
    {
        // Arrange
        var filePath = "test.json";
        var jsonPath = "$.data";
        _mockParserService.Setup(x => x.SearchJsonFile(filePath, jsonPath, true, true))
            .Returns("result");

        // Act
        var result = await _parserTools.SearchJson(filePath, jsonPath, true, true);

        // Assert
        Assert.Contains("\"success\": true", result);
        _mockParserService.Verify(x => x.SearchJsonFile(filePath, jsonPath, true, true), Times.Once);
    }

    [Fact]
    public async Task SearchCsv_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = "data.csv";
        var jsonPath = "$[0].Name";
        var expectedResult = "John";
        _mockParserService.Setup(x => x.SearchCsvFile(filePath, jsonPath, true, true))
            .Returns(expectedResult);

        // Act
        var result = await _parserTools.SearchCsv(filePath, jsonPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(filePath, result);
        _mockParserService.Verify(x => x.SearchCsvFile(filePath, jsonPath, true, true), Times.Once);
    }

    [Fact]
    public async Task SearchXml_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = "data.xml";
        var xPath = "//item[@id='1']";
        var expectedResult = "<item id='1'>Value</item>";
        _mockParserService.Setup(x => x.SearchXmlFile(filePath, xPath, true, false))
            .Returns(expectedResult);

        // Act
        var result = await _parserTools.SearchXml(filePath, xPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(filePath, result);
        Assert.Contains(xPath, result);
        _mockParserService.Verify(x => x.SearchXmlFile(filePath, xPath, true, false), Times.Once);
    }

    [Fact]
    public async Task SearchYaml_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = "config.yaml";
        var jsonPath = "$.settings.port";
        var expectedResult = "8080";
        _mockParserService.Setup(x => x.SearchYamlFile(filePath, jsonPath, true, false))
            .Returns(expectedResult);

        // Act
        var result = await _parserTools.SearchYaml(filePath, jsonPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(filePath, result);
        _mockParserService.Verify(x => x.SearchYamlFile(filePath, jsonPath, true, false), Times.Once);
    }

    [Fact]
    public async Task SearchExcel_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = "data.xlsx";
        var jsonPath = "$[0].Column1";
        var expectedResult = "Value1";
        _mockParserService.Setup(x => x.SearchExcelFile(filePath, jsonPath))
            .Returns(expectedResult);

        // Act
        var result = await _parserTools.SearchExcel(filePath, jsonPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(filePath, result);
        _mockParserService.Verify(x => x.SearchExcelFile(filePath, jsonPath), Times.Once);
    }

    [Fact]
    public async Task TransformXml_WithValidFiles_ReturnsSuccess()
    {
        // Arrange
        var xmlFilePath = "input.xml";
        var xsltFilePath = "transform.xslt";
        var expectedResult = "<transformed>Output</transformed>";
        _mockParserService.Setup(x => x.TransformXmlWithXslt(xmlFilePath, xsltFilePath, null, null))
            .Returns(expectedResult);

        // Act
        var result = await _parserTools.TransformXml(xmlFilePath, xsltFilePath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(xmlFilePath, result);
        Assert.Contains(xsltFilePath, result);
        _mockParserService.Verify(x => x.TransformXmlWithXslt(xmlFilePath, xsltFilePath, null, null), Times.Once);
    }

    [Fact]
    public async Task TransformXml_WithDestination_PassesParameter()
    {
        // Arrange
        var xmlFilePath = "input.xml";
        var xsltFilePath = "transform.xslt";
        var destinationPath = "output.xml";
        _mockParserService.Setup(x => x.TransformXmlWithXslt(xmlFilePath, xsltFilePath, destinationPath, null))
            .Returns("<result/>");

        // Act
        var result = await _parserTools.TransformXml(xmlFilePath, xsltFilePath, destinationPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(destinationPath, result);
        _mockParserService.Verify(x => x.TransformXmlWithXslt(xmlFilePath, xsltFilePath, destinationPath, null), Times.Once);
    }

    [Fact]
    public async Task SearchJson_WhenServiceReturnsNull_ReturnsError()
    {
        // Arrange
        var filePath = "invalid.json";
        var jsonPath = "$.data";
        _mockParserService.Setup(x => x.SearchJsonFile(filePath, jsonPath, true, false))
            .Returns((string?)null);

        // Act
        var result = await _parserTools.SearchJson(filePath, jsonPath);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains("error", result);
    }

    [Fact]
    public async Task SearchXml_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var filePath = "invalid.xml";
        var xPath = "//item";
        _mockParserService.Setup(x => x.SearchXmlFile(filePath, xPath, true, false))
            .Throws(new System.Xml.XmlException("Invalid XML"));

        // Act
        var result = await _parserTools.SearchXml(filePath, xPath);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains("error", result);
    }
}
