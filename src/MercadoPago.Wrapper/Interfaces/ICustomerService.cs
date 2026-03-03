using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Customers;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de clientes y tarjetas guardadas.</summary>
    public interface ICustomerService
    {
        Task<MpApiResponse<CustomerResponse>> CreateAsync(CustomerCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<CustomerResponse>> GetAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<MpPaginatedResponse<CustomerResponse>>> SearchAsync(CustomerSearchRequest criteria, CancellationToken ct = default);
        Task<MpApiResponse<CustomerResponse>> UpdateAsync(string id, CustomerCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<object>> DeleteAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<CardResponse>> SaveCardAsync(string customerId, CardCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<List<CardResponse>>> GetCardsAsync(string customerId, CancellationToken ct = default);
        Task<MpApiResponse<object>> DeleteCardAsync(string customerId, string cardId, CancellationToken ct = default);
    }
}
