using AbpTemplate.Entities;
using AbpTemplate.Services.Dtos;
using AutoMapper;

namespace AbpTemplate.ObjectMapping;

public class AbpTemplateAutoMapperProfile : Profile
{
    public AbpTemplateAutoMapperProfile()
    {
        /* Create your AutoMapper object mappings here */
        CreateMap<Sample, Samplepostdto>().ReverseMap();
    }
}
