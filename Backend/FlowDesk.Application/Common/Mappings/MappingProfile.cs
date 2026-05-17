using AutoMapper;
using FlowDesk.Application.Features.Categories.DTOs;
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
                                : null));
           //Comment
           CreateMap<Comment, CommentResponseDto>();

        }
    }
}
