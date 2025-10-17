using AutoMapper;
using TasksManager.Api.DTOs.Tasks;
using TasksManager.Api.Models;
public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskUpdateRequest, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src=>src.Status))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src=>src.DueDate))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src=>src.Title))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src=>src.Priority))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src=>src.Description));
    }
}
