using System.Collections.Generic;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnonymousComplaintsAPI.Mappers
{
    public static class AnonymousComplaintMapper
    {
        public static AnonymousComplaintResponse ToResponse(AnonymousComplaint entity, List<ShortProfileResponseDto> filtredProfiles)
        {
            if (entity == null)
                return null;

            var user = filtredProfiles?.FirstOrDefault(u => u.UserID == entity.CreatedBy);

            UserResponse newUser = null;

            if (user != null)
            {
                newUser = new UserResponse
                {
                    UserId = user.UserID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Identifier = user.Identifier,
                    Avatar = user.Avatar
                };
            }

            // Construction de la réponse principale
            var response = new AnonymousComplaintResponse
            {
                Id = entity.AnonymousComplaintId,
                Description = entity.Description,
                CategoryID = entity.CategoryId,
                CategoryName = entity.Category?.Name ?? "Non catégorisé",
                TypeID = entity.TypeId,
                TypeName = entity.Type?.Name ?? "Non typé",
                FrequencyID = entity.FrequencyId,
                State = entity.State,
                Archived = entity.Archived,
                IncidentDate = entity.IncidentDate,
                FusionWithId = entity.FusionWithId,
                CreatedAt = entity.CreatedAt,
                IsIdentityVisible = entity.IsIdentityVisible,


                Attachments = entity.Attachments != null
                    ? AttachmentMapper.ToResponseList(entity.Attachments).ToList()
                    : new List<AttachmentResponse>(),
                Solutions = entity.Solutions != null
            ? SolutionMapper.ToResponseList(entity.Solutions, filtredProfiles).ToList()
            : new List<SolutionResponse>()
            };

            // Si ce n’est pas une réclamation, on envoie le user
            if (entity.IsIdentityVisible == true || entity.TypeId != 34 )
            {
                response.CreatedBy = entity.CreatedBy;
                response.CreatedByUser = newUser;
            }
            else
            {
                response.CreatedBy = null ;
                response.CreatedByUser = null;
            }

            return response;
        }


        public static IEnumerable<AnonymousComplaintResponse> ToResponseList(IEnumerable<AnonymousComplaint> entities,List<ShortProfileResponseDto> filtredProfiles)
        {
            return entities?.Select(e => ToResponse(e, filtredProfiles))
                   ?? Enumerable.Empty<AnonymousComplaintResponse>();
        }


        public static AnonymousComplaint ToEntity(CreateAnonymousComplaintRequest request, int? userId = null)
        {
            if (request == null)
                return null;

            return new AnonymousComplaint
            {
                Description = request.Description,
                CategoryId = request.CategoryID,
                TypeId = request.TypeID,
                FrequencyId = request.FrequencyID,
                State = request.State,
                IncidentDate = request.IncidentDate,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = userId ?? request.CreatedBy,
                IsIdentityVisible = request.IsIdentityVisible ?? false,
                Archived = false
            };
        }

        public static ComplaintsByUserResponse ToComplaintsByUserResponse(
            FullProfileResponseDto? userProfile,
            IEnumerable<AnonymousComplaint> complaints)
        {
            var response = new ComplaintsByUserResponse
            {
                User = userProfile == null
                    ? null
                    : new UserProfileDataResponse
                    {
                        UserID = userProfile.UserID,
                        FirstName = userProfile.FirstName,
                        LastName = userProfile.LastName,
                        Identifier = userProfile.Matricule,
                        Avatar = userProfile.Avatar,
                        Email = userProfile.Email,
                        Phone = userProfile.phonePro,
                        Gender = userProfile.Gender,
                        IsActive = userProfile.Archive
                    },
                Data = complaints?.Select(entity =>
                {
                    var complaintItem = new AnonymousComplaintByUserItemResponse
                    {
                        Id = entity.AnonymousComplaintId,
                        Description = entity.Description,
                        CategoryID = entity.CategoryId,
                        CategoryName = entity.Category?.Name ?? "Non catégorisé",
                        TypeID = entity.TypeId,
                        TypeName = entity.Type?.Name ?? "Non typé",
                        FrequencyID = entity.FrequencyId,
                        State = entity.State,
                        Archived = entity.Archived,
                        IncidentDate = entity.IncidentDate,
                        FusionWithId = entity.FusionWithId,
                        CreatedAt = entity.CreatedAt,
                        IsIdentityVisible = entity.IsIdentityVisible,
                        Attachments = entity.Attachments != null
                            ? AttachmentMapper.ToResponseList(entity.Attachments).ToList()
                            : new List<AttachmentResponse>(),
                        Solutions = entity.Solutions != null
                            ? entity.Solutions.Select(s => new SolutionResponse
                            {
                                SolutionID = s.SolutionId,
                                Content = s.Content,
                                AnonymousComplaintID = s.AnonymousComplaintId,
                                CreatedBy = s.CreatedBy,
                                CreatedAt = s.CreatedAt,
                                Archived = s.Archived,
                                CreatedByUser = null
                            }).ToList()
                            : new List<SolutionResponse>()
                    };

                    if (entity.IsIdentityVisible == true || entity.TypeId != 34)
                    {
                        complaintItem.CreatedBy = entity.CreatedBy;
                    }
                    else
                    {
                        complaintItem.CreatedBy = null;
                    }

                    return complaintItem;
                }).ToList() ?? new List<AnonymousComplaintByUserItemResponse>()
            };

            return response;
        }


        public static void UpdateEntity(AnonymousComplaint entity, CreateAnonymousComplaintRequest request, int? userId = null)
        {
            if (entity == null || request == null)
                return;

            entity.Description = request.Description;
            entity.CategoryId = request.CategoryID;
            entity.TypeId = request.TypeID;
            entity.FrequencyId = request.FrequencyID;
            entity.IncidentDate = request.IncidentDate;
            entity.IsIdentityVisible = request.IsIdentityVisible ?? entity.IsIdentityVisible;
        }

        public static AnonymousComplaintResponse ToResponseWithDetails(AnonymousComplaint entity, List<ShortProfileResponseDto> filtredProfiles)

        {
            var response = ToResponse(entity,filtredProfiles);
            return response;
        }
    }
}
