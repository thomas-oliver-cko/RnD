using AutoMapper;
using Rnd.Core.ConsoleApp.DataGenerator;

namespace Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.DataObject
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EntityDetails, DynamoDbEntityDetails>().ReverseMap();
            CreateMap<Entity, DynamoDbEntity>().ReverseMap();
        }
    }
}
