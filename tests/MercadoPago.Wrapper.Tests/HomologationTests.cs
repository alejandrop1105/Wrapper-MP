using System.Collections.Generic;
using MercadoPago.Wrapper.Configuration;
using MercadoPago.Wrapper.Helpers;
using MercadoPago.Wrapper.Models.Orders;
using Newtonsoft.Json;
using Xunit;

namespace MercadoPago.Wrapper.Tests
{
    /// <summary>Tests para AmountFormatter.</summary>
    public class AmountFormatterTests
    {
        [Theory]
        [InlineData(1500.50, "AR", "1500.50")]
        [InlineData(1500.50, "BR", "1500.50")]
        [InlineData(1500.50, "MX", "1500.50")]
        [InlineData(1500.00, "AR", "1500.00")]
        [InlineData(0.99, "AR", "0.99")]
        public void Format_WithDecimalCountry_ReturnsTwoDecimals(
            double amount, string country, string expected)
        {
            var result = AmountFormatter.Format((decimal)amount, country);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1500.50, "CL", "1500")]
        [InlineData(1500.99, "CO", "1500")]
        [InlineData(1000.00, "CL", "1000")]
        public void Format_WithNoDecimalCountry_TruncatesDecimals(
            double amount, string country, string expected)
        {
            var result = AmountFormatter.Format((decimal)amount, country);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1500.50", "AR", true)]
        [InlineData("1500.50", "CL", false)]
        [InlineData("1500", "CL", true)]
        [InlineData("", "AR", false)]
        [InlineData(null, "AR", false)]
        public void IsValidFormat_ReturnsExpected(
            string amountString, string country, bool expected)
        {
            var result = AmountFormatter.IsValidFormat(amountString, country);
            Assert.Equal(expected, result);
        }
    }

    /// <summary>Tests para ExternalReferenceValidator.</summary>
    public class ExternalReferenceValidatorTests
    {
        [Theory]
        [InlineData("VENTA-001")]
        [InlineData("ORD-2024-12345")]
        [InlineData("POS-A-TICKET-789")]
        [InlineData(null)]
        [InlineData("")]
        public void Validate_SafeReferences_ReturnsTrue(string reference)
        {
            Assert.True(
                ExternalReferenceValidator.Validate(reference));
        }

        [Theory]
        [InlineData("juan@email.com")]
        [InlineData("VENTA-juan@email.com-001")]
        public void Validate_ContainsEmail_ReturnsFalse(string reference)
        {
            Assert.False(
                ExternalReferenceValidator.Validate(reference));
        }

        [Theory]
        [InlineData("20-12345678-9")]
        [InlineData("CUIT-20123456789")]
        public void Validate_ContainsDniCuit_ReturnsFalse(string reference)
        {
            Assert.False(
                ExternalReferenceValidator.Validate(reference));
        }
    }

    /// <summary>Tests para OrderCreateRequest (serialización).</summary>
    public class OrderModelsTests
    {
        [Fact]
        public void OrderCreateRequest_SerializesWithPlatformId()
        {
            var request = new OrderCreateRequest
            {
                Type = "online",
                TotalAmount = "5000.00",
                ExternalReference = "TEST-001",
                PlatformId = "mp-platform-123"
            };

            var json = JsonConvert.SerializeObject(request);
            Assert.Contains("\"platform_id\":\"mp-platform-123\"", json);
        }

        [Fact]
        public void OrderCreateRequest_SerializesConfigPoint()
        {
            var request = new OrderCreateRequest
            {
                TotalAmount = "1000.00",
                Config = new OrderConfigRequest
                {
                    Point = new OrderPointConfigRequest
                    {
                        PrintOnTerminal = true,
                        TicketNumber = "T-001"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(request);
            Assert.Contains("\"print_on_terminal\":true", json);
            Assert.Contains("\"ticket_number\":\"T-001\"", json);
        }

        [Fact]
        public void OrderCreateRequest_OmitsNullConfigPoint()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var request = new OrderCreateRequest
            {
                TotalAmount = "1000.00",
                ExternalReference = "TEST-002"
            };

            var json = JsonConvert.SerializeObject(request, settings);
            Assert.DoesNotContain("config", json);
            Assert.DoesNotContain("platform_id", json);
        }
    }

    /// <summary>Tests para MpWrapperConfig Builder.</summary>
    public class ConfigTests
    {
        [Fact]
        public void Builder_WithPlatformId_SetsProperty()
        {
            var config = new MpWrapperConfig.Builder()
                .WithAccessToken("TEST_TOKEN")
                .WithPlatformId("mp-plat-123")
                .Build();

            Assert.Equal("mp-plat-123", config.PlatformId);
        }

        [Fact]
        public void Builder_WithOAuthConfig_SetsProperties()
        {
            var config = new MpWrapperConfig.Builder()
                .WithAccessToken("TEST_TOKEN")
                .WithClientId("client_id_123")
                .WithClientSecret("client_secret_456")
                .WithRefreshToken("refresh_789")
                .Build();

            Assert.Equal("client_id_123", config.ClientId);
            Assert.Equal("client_secret_456", config.ClientSecret);
            Assert.Equal("refresh_789", config.RefreshToken);
        }

        [Fact]
        public void Builder_WithCountry_SetsProperty()
        {
            var config = new MpWrapperConfig.Builder()
                .WithAccessToken("TEST_TOKEN")
                .WithCountry("CL")
                .Build();

            Assert.Equal("CL", config.Country);
        }
    }
}
