using AutoMapper;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //User
            CreateMap<CreateUserDto, User>();
            CreateMap<User, UserResponseDto>();
            CreateMap<UpdateUserDetailsDto, User>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            //Category
            CreateMap<Category, CategoryAdminResponseDto>();
            CreateMap<Category, CategoryBasicResponseDto>();
            CreateMap<CategoryResponseDto, CategoryAdminResponseDto>();
            CreateMap<CategoryResponseDto, CategoryBasicResponseDto>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<Category, CategoryResponseDto>();

            //Request
            CreateMap<CreateRequestDto, Request>();
            CreateMap<Request, EmployeeRequestResponseDto>();
            CreateMap<Request, RequestResponseDto>()
                        .ForMember(d => d.FullName,
                            opt => opt.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
                        .ForMember(d => d.CategoryName,
                            opt => opt.MapFrom(s => s.Category != null ? s.Category.CategoryName : null))
                        .ForMember(d => d.AssignedUser,
                            opt => opt.MapFrom(s => s.AssignedUser != null ? s.AssignedUser.FullName : null))
                        .ForMember(d => d.ApprovalName,
                            opt => opt.MapFrom(s => s.Employee != null && s.Employee.Manager != null
                                ? s.Employee.Manager.FullName
                                : null))
                        .ForMember(d => d.EscalatedByName,
                            opt => opt.MapFrom(s => s.EscalatedByUser != null ? s.EscalatedByUser.FullName : null))
                        .ForMember(d => d.SlaStatus,
                            opt => opt.Ignore());
            CreateMap<Request, RequestDetailDto>()
                        .IncludeBase<Request, RequestResponseDto>()
                        .ForMember(d => d.EscalationHistory,
                            opt => opt.MapFrom(s => s.EscalationHistory));
            CreateMap<EscalationHistory, EscalationHistoryDto>()
                        .ForMember(d => d.EscalatedByName,
                            opt => opt.MapFrom(s => s.EscalatedByUser != null ? s.EscalatedByUser.FullName : string.Empty));
            //Comment
            CreateMap<Comment, CommentResponseDto>();

            //Remote session
            CreateMap<RemoteSession, RemoteSessionDto>()
                    .ForMember(dest => dest.Id,
                        opt => opt.MapFrom(src => src.RemoteSessionId))

                    .ForMember(dest => dest.InitiatedByName,
                        opt => opt.MapFrom(src => src.InitiatedBy != null
                            ? src.InitiatedBy.FullName
                            : string.Empty))

                    .ForMember(dest => dest.TargetUserName,
                        opt => opt.MapFrom(src => src.TargetUser != null
                            ? src.TargetUser.FullName
                            : string.Empty))

                    .ForMember(dest => dest.Status,
                        opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<RemoteSession, RemoteSessionDto>()
    .ForMember(d => d.Id,
        o => o.MapFrom(s => s.RemoteSessionId))

    .ForMember(d => d.InitiatedByName,
        o => o.MapFrom(s => s.Request.AssignedUser.FullName))

    .ForMember(d => d.TargetUserName,
        o => o.MapFrom(s => s.Request.Employee.FullName))

    .ForMember(d => d.Status,
        o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}
