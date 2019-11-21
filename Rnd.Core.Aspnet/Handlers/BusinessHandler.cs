using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rnd.Core.Aspnet.Domain;
using Rnd.Core.Aspnet.Repo;

namespace Rnd.Core.Aspnet.Handlers
{
    public class BusinessHandler : IRequestHandler<Command<BusinessModel>, Result>
    {
        readonly IBusinessRepository repo;

        public BusinessHandler(IBusinessRepository repo)
        {
            this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<Result> Handle(Command<BusinessModel> request, CancellationToken cancellationToken)
        {
            await repo.SaveAsync(request.Data);
            return Result.Ok(request.Data);
        }
    }
}
