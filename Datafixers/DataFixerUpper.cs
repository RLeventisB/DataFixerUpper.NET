// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers;

using com.google.common.collect.Lists;
using DataFixerUpper.Datafixers.functions.PointFreeRule;
using DataFixerUpper.Datafixers.schemas.Schema;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.Dynamic;
using it.unimi.dsi.fastutil.ints.Int2ObjectSortedMap;
using it.unimi.dsi.fastutil.ints.IntSortedSet;
using it.unimi.dsi.fastutil.longs.Long2ObjectMap;
using it.unimi.dsi.fastutil.longs.Long2ObjectMaps;
using it.unimi.dsi.fastutil.longs.Long2ObjectOpenHashMap;
using org.slf4j.Logger;
using org.slf4j.LoggerFactory;

using java.util.List;

/*
 * Optimizing functions
 *   Cunha, A., & Pinto, J. S. (2005). Point-free program transformation
 *   Lämmel, R., Visser, E., & Visser, J. (2002). The essence of strategic programming
 *
 * How to handle recursive types
 *   Cunha, A., & Pacheco, H. (2011). Algebraic specialization of generic functions for recursive types
 *   Yakushev, A. R., Holdermans, S., Löh, A., & Jeuring, J. (2009, August). Generic programming with fixed points for mutually recursive datatypes
 *   Magalhães, J. P., & Löh, A. (2012). A formal comparison of approaches to datatype-generic programming
 *
 * Optics
 *   Pickering, M., Gibbons, J., & Wu, N. (2017). Profunctor Optics: Modular Data Accessors
 *   Pacheco, H., & Cunha, A. (2010, June). Generic point-free lenses
 *
 * Tying it together
 *   Cunha, A., Oliveira, J. N., & Visser, J. (2006, August). Type-safe two-level data transformation
 *   Cunha, A., & Visser, J. (2011). Transformation of structure-shy programs with application to XPath queries and strategic functions
 *   Pacheco, H., & Cunha, A. (2011, January). Calculating with lenses: optimising bidirectional transformations
 */
public class DataFixerUpper implements DataFixer {
    public static boolean ERRORS_ARE_FATAL = false;

    private static final Logger LOGGER = LoggerFactory.getLogger(DataFixerUpper.class);

    protected static final PointFreeRule OPTIMIZATION_RULE = DataFixUtils.make(() -> PointFreeRule.everywhere(
        // Top-down: these rules produce new compositions that also need to be rewritten
        PointFreeRule.seq(
            // Applying CataFuseDifferent before CataFuseSame would prevent some merges from happening, but not the other way around
            PointFreeRule.CataFuseSame.INSTANCE,
            PointFreeRule.CataFuseDifferent.INSTANCE,
            // Apply all of these together exhaustively because each change can allow another rule to apply
            PointFreeRule.CompRewrite.together(
                // Merge functions applying to identical optics, must run before merging nested applied functions
                PointFreeRule.LensComp.INSTANCE,
                PointFreeRule.SortProj.INSTANCE,
                PointFreeRule.SortInj.INSTANCE
            )
        ),
        // Bottom-up: ensure we nest the full tree in a single pass
        PointFreeRule.AppNest.INSTANCE
    ));

    private final Int2ObjectSortedMap<Schema> schemas;
    private final List<DataFix> globalList;
    private final IntSortedSet fixerVersions;
    private final Long2ObjectMap<TypeRewriteRule> rules = Long2ObjectMaps.synchronize(new Long2ObjectOpenHashMap<>());

    protected DataFixerUpper(final Int2ObjectSortedMap<Schema> schemas, final List<DataFix> globalList, final IntSortedSet fixerVersions) {
        this.schemas = schemas;
        this.globalList = globalList;
        this.fixerVersions = fixerVersions;
    }

    @Override
    public <T> Dynamic<T> update(final DSL.TypeReference type, final Dynamic<T> input, final int version, final int newVersion) {
        if (version < newVersion) {
            final Type<?> dataType = getType(type, version);
            final DataResult<T> read = dataType.readAndWrite(input.getOps(), getType(type, newVersion), getRule(version, newVersion), OPTIMIZATION_RULE, input.getValue());
            final T result = read.resultOrPartial(LOGGER::error).orElse(input.getValue());
            return new Dynamic<>(input.getOps(), result);
        }
        return input;
    }

    @Override
    public Schema getSchema(final int key) {
        return schemas.get(getLowestSchemaSameVersion(schemas, key));
    }

    protected Type<?> getType(final DSL.TypeReference type, final int version) {
        return getSchema(DataFixUtils.makeKey(version)).getTypeRaw(type);
    }

    protected static int getLowestSchemaSameVersion(final Int2ObjectSortedMap<Schema> schemas, final int versionKey) {
        if (versionKey < schemas.firstIntKey()) {
            // can't have a data type before anything else
            return schemas.firstIntKey();
        }
        return schemas.subMap(0, versionKey + 1).lastIntKey();
    }

    private int getLowestFixSameVersion(final int versionKey) {
        if (versionKey < fixerVersions.firstInt()) {
            // can have a version before everything else
            return fixerVersions.firstInt() - 1;
        }
        return fixerVersions.subSet(0, versionKey + 1).lastInt();
    }

    protected TypeRewriteRule getRule(final int version, final int newVersion) {
        if (version >= newVersion) {
            return TypeRewriteRule.nop();
        }

        final long key = (long) version << 32 | newVersion;
        return rules.computeIfAbsent(key, k -> {
            final int expandedVersion = getLowestFixSameVersion(DataFixUtils.makeKey(version));

            final List<TypeRewriteRule> rules = Lists.newArrayList();
            for (final DataFix fix : globalList) {
                final int expandedFixVersion = fix.getVersionKey();
                final int fixVersion = DataFixUtils.getVersion(expandedFixVersion);
                if (expandedFixVersion > expandedVersion && fixVersion <= newVersion) {
                    final TypeRewriteRule fixRule = fix.getRule();
                    if (fixRule == TypeRewriteRule.nop()) {
                        continue;
                    }
                    rules.add(fixRule);
                }
            }

            return TypeRewriteRule.seq(rules);
        });
    }

    protected IntSortedSet fixerVersions() {
        return fixerVersions;
    }
}
