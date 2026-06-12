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
                         .ForMember(d => d.RemoteSessionId,
                             o => o.MapFrom(s => s.RemoteSessionId))

                         .ForMember(d => d.RequestId,
                             o => o.MapFrom(s => s.RequestId))

                         .ForMember(d => d.InitiatedByUserId,
                             o => o.MapFrom(s => s.InitiatedByUserId))

                         .ForMember(d => d.TargetUserId,
                             o => o.MapFrom(s => s.TargetUserId))

                         // Names (SAFE null handling)
                         .ForMember(d => d.InitiatedByName,
                             o => o.MapFrom(s =>
                                 s.InitiatedBy != null
                                     ? s.InitiatedBy.FullName
                                     : s.Request != null && s.Request.AssignedUser != null
                                         ? s.Request.AssignedUser.FullName
                                         : string.Empty))

                         .ForMember(d => d.TargetUserName,
                             o => o.MapFrom(s =>
                                 s.TargetUser != null
                                     ? s.TargetUser.FullName
                                     : s.Request != null && s.Request.Employee != null
                                         ? s.Request.Employee.FullName
                                         : string.Empty))

                         // Enum → string
                         .ForMember(d => d.Status,
                             o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}
