using AutoMapper;
using Rnd.Core.ConsoleApp.DataGenerator;
using Rnd.Core.ConsoleApp.DataGenerator.Entities;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v3.Database.Dynamo.DataObject
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
