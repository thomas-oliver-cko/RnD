using System;
using MediatR;

namespace Rnd.Core.Aspnet.Handlers
{
    public class Command<T> : IRequest<Result>
    {
        public Command(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
        public string CommandId => Guid.NewGuid().ToString();
    }
}
