using AutoMapper;
using Rnd.Core.ConsoleApp.DataGenerator;

namespace Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo.DataObject
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Entity, DynamoDbEntity>().ReverseMap();
            CreateMap<Details, DynamoDbEntityDetails>().ReverseMap();
        }
    }
}
