using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommandSurfacer.Tests;

public class StringConverterTests : BaseTests
{
    private readonly IStringConverter _stringConverter;

    public StringConverterTests(CommandSurfacerTestFixture fixture)
    {
        _stringConverter = fixture.AppHost.Services.GetRequiredService<IStringConverter>();
    }

    #region FileInfo
    [Fact]
    public void StringToFileInfo_FileExists()
    {
        var path = ".\\Sandbox\\TestFile.txt";
        var file = _stringConverter.Convert<FileInfo>(path);

        Assert.NotNull(file);
        Assert.True(file.Exists);
        Assert.Equal("TestFile.txt", file.Name);
    }

    [Fact]
    public void StringToFileInfo_FileNotExists()
    {
        var path = ".\\Sandbox\\NotExists.txt";
        var file = _stringConverter.Convert<FileInfo>(path);

        Assert.NotNull(file);
        Assert.False(file.Exists);
        Assert.Equal("NotExists.txt", file.Name);
    }
    #endregion

    #region DirectoryInfo
    [Fact]
    public void StringToFileInfo_DirectoryExists()
    {
        var path = ".\\Sandbox";
        var file = _stringConverter.Convert<DirectoryInfo>(path);

        Assert.NotNull(file);
        Assert.True(file.Exists);
        Assert.Equal("Sandbox", file.Name);
    }

    [Fact]
    public void StringToFileInfo_DirectoryNotExists()
    {
        var path = ".\\NotExists";
        var file = _stringConverter.Convert<DirectoryInfo>(path);

        Assert.NotNull(file);
        Assert.False(file.Exists);
        Assert.Equal("NotExists", file.Name);
    }
    #endregion

    #region String
    [Fact]
    public void StringToString_WithValue()
    {
        var expected = "testing string";
        var output = _stringConverter.Convert<string>(expected);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToString_EmptyValue()
    {
        var expected = string.Empty;
        var output = _stringConverter.Convert<string>(expected);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToString_NullValue()
    {
        var input = default(string);
        var output = _stringConverter.Convert<string>(input);
        Assert.Null(output);
    }
    #endregion

    #region Byte
    [Fact]
    public void StringToByte_MaxValue()
    {
        var expected = byte.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<byte>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToByte_MinValue()
    {
        var expected = byte.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<byte>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToByte_EmptyString_ThrowsException()
    {
        var input = string.Empty;
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<byte>(input));
    }

    [Fact]
    public void StringToByte_MaxValuePlusOne_ThrowsException()
    {
        var input = byte.MaxValue.ToString() + "1";
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<byte>(input));
    }

    [Fact]
    public void StringToNullableByte_MaxValue()
    {
        var expected = byte.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<byte?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableByte_MinValue()
    {
        var expected = byte.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<byte?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableByte_EmptyValue_NotThrowsException()
    {
        var input = string.Empty;
        var output = _stringConverter.Convert<byte?>(input);
        Assert.Null(output);
    }

    [Fact]
    public void StringToNullableByte_MaxValuePlusOne_NotThrowsException()
    {
        var input = byte.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<byte?>(input);
        Assert.Null(output);
    }
    #endregion

    #region Short
    [Fact]
    public void StringToShort_MaxValue()
    {
        var expected = short.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<short>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToShort_MinValue()
    {
        var expected = short.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<short>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToShort_EmptyString_ThrowsException()
    {
        var input = string.Empty;
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<short>(input));
    }

    [Fact]
    public void StringToShort_MaxValuePlusOne_ThrowsException()
    {
        var input = short.MaxValue.ToString() + "1";
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<short>(input));
    }

    [Fact]
    public void StringToNullableShort_MaxValue()
    {
        var expected = short.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<short?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableShort_MinValue()
    {
        var expected = short.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<short?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableShort_EmptyValue_NotThrowsException()
    {
        var input = string.Empty;
        var output = _stringConverter.Convert<short?>(input);
        Assert.Null(output);
    }

    [Fact]
    public void StringToNullableShort_MaxValuePlusOne_NotThrowsException()
    {
        var input = short.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<short?>(input);
        Assert.Null(output);
    }
    #endregion

    #region Int
    [Fact]
    public void StringToInt_MaxValue()
    {
        var expected = int.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<int>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToInt_MinValue()
    {
        var expected = int.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<int>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToInt_EmptyString_ThrowsException()
    {
        var input = string.Empty;
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<int>(input));
    }

    [Fact]
    public void StringToInt_MaxValuePlusOne_ThrowsException()
    {
        var input = int.MaxValue.ToString() + "1";
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<int>(input));
    }

    [Fact]
    public void StringToNullableInt_MaxValue()
    {
        var expected = int.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<int?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableInt_MinValue()
    {
        var expected = int.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<int?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableInt_EmptyValue_NotThrowsException()
    {
        var input = string.Empty;
        var output = _stringConverter.Convert<int?>(input);
        Assert.Null(output);
    }

    [Fact]
    public void StringToNullableInt_MaxValuePlusOne_NotThrowsException()
    {
        var input = int.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<int?>(input);
        Assert.Null(output);
    }
    #endregion

    #region Long
    [Fact]
    public void StringToLong_MaxValue()
    {
        var expected = long.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<long>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToLong_MinValue()
    {
        var expected = long.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<long>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToLong_EmptyString_ThrowsException()
    {
        var input = string.Empty;
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<long>(input));
    }

    [Fact]
    public void StringToLong_MaxValuePlusOne_ThrowsException()
    {
        var input = long.MaxValue.ToString() + "1";
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<long>(input));
    }

    [Fact]
    public void StringToNullableLong_MaxValue()
    {
        var expected = long.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<long?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableLong_MinValue()
    {
        var expected = long.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<long?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableLong_EmptyValue_NotThrowsException()
    {
        var input = string.Empty;
        var output = _stringConverter.Convert<long?>(input);
        Assert.Null(output);
    }

    [Fact]
    public void StringToNullableLong_MaxValuePlusOne_NotThrowsException()
    {
        var input = long.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<long?>(input);
        Assert.Null(output);
    }
    #endregion

    #region Double
    [Fact]
    public void StringToDouble_MaxValue()
    {
        var expected = double.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<double>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToDouble_MinValue()
    {
        var expected = double.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<double>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToDouble_EmptyString_ThrowsException()
    {
        var input = string.Empty;
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<double>(input));
    }

    [Fact]
    public void StringToDouble_MaxValuePlusOne_ThrowsException()
    {
        var input = double.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<double>(input);
        Assert.Equal(double.PositiveInfinity, output);

    }

    [Fact]
    public void StringToNullableDouble_MaxValue()
    {
        var expected = double.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<double?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableDouble_MinValue()
    {
        var expected = double.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<double?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableDouble_EmptyValue_NotThrowsException()
    {
        var input = string.Empty;
        var output = _stringConverter.Convert<double?>(input);
        Assert.Null(output);
    }

    [Fact]
    public void StringToNullableDouble_MaxValuePlusOne_NotThrowsException()
    {
        var input = double.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<double?>(input);
        Assert.Equal(double.PositiveInfinity, output);
    }
    #endregion

    #region Float
    [Fact]
    public void StringToFloat_MaxValue()
    {
        var expected = float.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<float>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToFloat_MinValue()
    {
        var expected = float.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<float>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToFloat_EmptyString_ThrowsException()
    {
        var input = string.Empty;
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<float>(input));
    }

    [Fact]
    public void StringToFloat_MaxValuePlusOne_ThrowsException()
    {
        var input = float.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<float>(input);
        Assert.Equal(float.PositiveInfinity, output);
    }

    [Fact]
    public void StringToNullableFloat_MaxValue()
    {
        var expected = float.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<float?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableFloat_MinValue()
    {
        var expected = float.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<float?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableFloat_EmptyValue_NotThrowsException()
    {
        var input = string.Empty;
        var output = _stringConverter.Convert<float?>(input);
        Assert.Null(output);
    }

    [Fact]
    public void StringToNullableFloat_MaxValuePlusOne_NotThrowsException()
    {
        var input = float.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<float?>(input);
        Assert.Equal(float.PositiveInfinity, output);
    }
    #endregion

    #region Decimal
    [Fact]
    public void StringToDecimal_MaxValue()
    {
        var expected = decimal.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<decimal>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToDecimal_MinValue()
    {
        var expected = decimal.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<decimal>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToDecimal_EmptyString_ThrowsException()
    {
        var input = string.Empty;
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<decimal>(input));
    }

    [Fact]
    public void StringToDecimal_MaxValuePlusOne_ThrowsException()
    {
        var input = decimal.MaxValue.ToString() + "1";
        Assert.ThrowsAny<Exception>(() => _stringConverter.Convert<decimal>(input));
    }

    [Fact]
    public void StringToNullableDecimal_MaxValue()
    {
        var expected = decimal.MaxValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<decimal?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableDecimal_MinValue()
    {
        var expected = decimal.MinValue;
        var input = expected.ToString();
        var output = _stringConverter.Convert<decimal?>(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void StringToNullableDecimal_EmptyValue_NotThrowsException()
    {
        var input = string.Empty;
        var output = _stringConverter.Convert<decimal?>(input);
        Assert.Null(output);
    }

    [Fact]
    public void StringToNullableDecimal_MaxValuePlusOne_NotThrowsException()
    {
        var input = decimal.MaxValue.ToString() + "1";
        var output = _stringConverter.Convert<decimal?>(input);
        Assert.Null(output);
    }
    #endregion

    #region Exceptions
    [Fact]
    public void StringToUnsupportedType_ThrowsException() => Assert.Throws<InvalidOperationException>(() => _stringConverter.Convert<StringConverterTests>(""));

    [Fact]
    public void StringToNumeric_InvalidFormatException_ThrowsException() => Assert.Throws<InvalidOperationException>(() => _stringConverter.Convert<decimal>("jake"));
    #endregion
}