using MediatR;

namespace Rnd.Core.Aspnet.Domain
{
    public class BusinessModel
    {
        public string Value { get; set; }
    }

    /*
     * Api - contracts
     *
     *
     * Mediatr business - domain models
     * Domain in
     * Domain out
     *
     *
     * DB - entities/pocos
     *
     */

    // API
    class CreateEntityRequest
    {

    }

    class CreateEntityResponse
    {

    }

    // DOMAIN BL
    class CreateEntity : IRequest<KycEntity>
    {

    }

    class KycEntity
    {
    }
}
