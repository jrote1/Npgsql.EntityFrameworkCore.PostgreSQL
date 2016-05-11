#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using Npgsql.TypeHandlers;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    // TODO: BIT(1) vs. BIT(N)
    // TODO: Enums - https://github.com/aspnet/EntityFramework/issues/3620
    // TODO: Arrays? But this would conflict with navigation...
    public class NpgsqlTypeMapper : RelationalTypeMapper
    {
        readonly Dictionary<string, RelationalTypeMapping> _simpleNameMappings;
        readonly Dictionary<Type, RelationalTypeMapping> _simpleMappings;

        public NpgsqlTypeMapper()
        {
            // First, PostgreSQL type name (string) -> RelationalTypeMapping
            _simpleNameMappings = TypeHandlerRegistry.HandlerTypes.Values
                // Base types
                .Where(tam => tam.Mapping.NpgsqlDbType.HasValue)
                .Select(tam => new {
                    Name = tam.Mapping.PgName,
                    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(tam.Mapping.PgName, GetTypeHandlerTypeArgument(tam.HandlerType), tam.Mapping.NpgsqlDbType.Value)
                })
                // Enums
                //.Concat(TypeHandlerRegistry.GlobalEnumMappings.Select(kv => new {
                //    Name = kv.Key,
                //    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((IEnumHandler)kv.Value).EnumType)
                //}))
                // Composites
                .Concat(TypeHandlerRegistry.GlobalCompositeMappings.Select(kv => new {
                    Name = kv.Key,
                    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((ICompositeHandler)kv.Value).CompositeType)
                }))
                // Output
                .ToDictionary(x => x.Name, x => x.Mapping);

            // Second, CLR type -> RelationalTypeMapping
            _simpleMappings = TypeHandlerRegistry.HandlerTypes.Values
                // Base types
                .Select(tam => tam.Mapping)
                .Where(m => m.NpgsqlDbType.HasValue)
                .SelectMany(m => m.ClrTypes, (m, t) => new {
                    Type = t,
                    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(m.PgName, t, m.NpgsqlDbType.Value)
                })
                // Enums
                //.Concat(TypeHandlerRegistry.GlobalEnumMappings.Select(kv => new {
                //    Type = ((IEnumHandler)kv.Value).EnumType,
                //    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((IEnumHandler)kv.Value).EnumType)
                //}))
                // Composites
                .Concat(TypeHandlerRegistry.GlobalCompositeMappings.Select(kv => new {
                    Type = ((ICompositeHandler)kv.Value).CompositeType,
                    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((ICompositeHandler)kv.Value).CompositeType)
                }))
                // Output
                .ToDictionary(x => x.Type, x => x.Mapping);
        }

        protected override string GetColumnType(IProperty property) => property.Npgsql().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetSimpleMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetSimpleNameMappings()
            => _simpleNameMappings;

        static Type GetTypeHandlerTypeArgument(Type handler)
        {
            while (!handler.GetTypeInfo().IsGenericType || handler.GetGenericTypeDefinition() != typeof(TypeHandler<>))
            {
                handler = handler.GetTypeInfo().BaseType;
                if (handler == null)
                {
                    throw new Exception("Npgsql type handler doesn't inherit from TypeHandler<>?");
                }
            }

            return handler.GetGenericArguments()[0];
        }
    }
}