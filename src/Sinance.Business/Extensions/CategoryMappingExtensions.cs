﻿using Sinance.Communication.Model.CategoryMapping;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.Business.Extensions;

public static class CategoryMappingExtensions
{
    public static List<CategoryMappingModel> ToDto(this List<CategoryMappingEntity> entities) => entities.Select(x => x.ToDto()).ToList();

    public static CategoryMappingModel ToDto(this CategoryMappingEntity entity)
    {
        return new CategoryMappingModel
        {
            Id = entity.Id,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category.Name,
            MatchValue = entity.MatchValue,
            ColumnTypeId = entity.ColumnTypeId
        };
    }

    public static CategoryMappingEntity ToNewEntity(this CategoryMappingModel model, int userId)
    {
        return new CategoryMappingEntity
        {
            CategoryId = model.CategoryId,
            ColumnTypeId = model.ColumnTypeId,
            MatchValue = model.MatchValue,
            UserId = userId
        };
    }

    public static CategoryMappingEntity UpdateEntityFromModel(this CategoryMappingEntity entity, CategoryMappingModel model)
    {
        entity.MatchValue = model.MatchValue;
        entity.ColumnTypeId = model.ColumnTypeId;
        entity.CategoryId = model.CategoryId;

        return entity;
    }
}