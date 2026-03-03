using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Customers;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de clientes y tarjetas guardadas.</summary>
    public class CustomerService : ICustomerService
    {
        private readonly MpHttpClient _http;

        public CustomerService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<CustomerResponse>> CreateAsync(
            CustomerCreateRequest request, CancellationToken ct = default)
        {
            return await _http.PostAsync<CustomerResponse>(
                "/v1/customers", request, ct: ct);
        }

        public async Task<MpApiResponse<CustomerResponse>> GetAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.GetAsync<CustomerResponse>(
                $"/v1/customers/{id}", ct);
        }

        public async Task<MpApiResponse<MpPaginatedResponse<CustomerResponse>>> SearchAsync(
            CustomerSearchRequest criteria, CancellationToken ct = default)
        {
            var qs = criteria?.ToQueryString() ?? string.Empty;
            var endpoint = string.IsNullOrEmpty(qs)
                ? "/v1/customers/search"
                : $"/v1/customers/search?{qs}";
            return await _http.GetAsync<MpPaginatedResponse<CustomerResponse>>(
                endpoint, ct);
        }

        public async Task<MpApiResponse<CustomerResponse>> UpdateAsync(
            string id, CustomerCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PutAsync<CustomerResponse>(
                $"/v1/customers/{id}", request, ct);
        }

        public async Task<MpApiResponse<object>> DeleteAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.DeleteAsync($"/v1/customers/{id}", ct);
        }

        public async Task<MpApiResponse<CardResponse>> SaveCardAsync(
            string customerId, CardCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<CardResponse>(
                $"/v1/customers/{customerId}/cards", request, ct: ct);
        }

        public async Task<MpApiResponse<List<CardResponse>>> GetCardsAsync(
            string customerId, CancellationToken ct = default)
        {
            return await _http.GetAsync<List<CardResponse>>(
                $"/v1/customers/{customerId}/cards", ct);
        }

        public async Task<MpApiResponse<object>> DeleteCardAsync(
            string customerId, string cardId,
            CancellationToken ct = default)
        {
            return await _http.DeleteAsync(
                $"/v1/customers/{customerId}/cards/{cardId}", ct);
        }
    }
}
