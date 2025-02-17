// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers;

using DataFixerUpper.Datafixers.schemas.Schema;
using DataFixerUpper.Datafixers.types.Type;
using it.unimi.dsi.fastutil.ints.Int2ObjectAVLTreeMap;
using it.unimi.dsi.fastutil.ints.Int2ObjectSortedMap;
using it.unimi.dsi.fastutil.ints.IntAVLTreeSet;
using it.unimi.dsi.fastutil.ints.IntIterator;
using it.unimi.dsi.fastutil.ints.IntSortedSet;
using org.slf4j.Logger;
using org.slf4j.LoggerFactory;

using java.time.Duration;
using java.time.Instant;
using java.util.ArrayList;
using java.util.List;
using java.util.Set;
using java.util.concurrent.CompletableFuture;
using java.util.concurrent.Executor;
using java.util.function.BiFunction;
using java.util.stream.Collectors;

public class DataFixerBuilder {
    private static final Logger LOGGER = LoggerFactory.getLogger(DataFixerBuilder.class);

    private final int dataVersion;
    private final Int2ObjectSortedMap<Schema> schemas = new Int2ObjectAVLTreeMap<>();
    private final List<DataFix> globalList = new ArrayList<>();
    private final IntSortedSet fixerVersions = new IntAVLTreeSet();

    public DataFixerBuilder(final int dataVersion) {
        this.dataVersion = dataVersion;
    }

    public Schema addSchema(final int version, final BiFunction<Integer, Schema, Schema> factory) {
        return addSchema(version, 0, factory);
    }

    public Schema addSchema(final int version, final int subVersion, final BiFunction<Integer, Schema, Schema> factory) {
        final int key = DataFixUtils.makeKey(version, subVersion);
        final Schema parent = schemas.isEmpty() ? null : schemas.get(DataFixerUpper.getLowestSchemaSameVersion(schemas, key - 1));
        final Schema schema = factory.apply(DataFixUtils.makeKey(version, subVersion), parent);
        addSchema(schema);
        return schema;
    }

    public void addSchema(final Schema schema) {
        schemas.put(schema.getVersionKey(), schema);
    }

    public void addFixer(final DataFix fix) {
        final int version = DataFixUtils.getVersion(fix.getVersionKey());

        if (version > dataVersion) {
            LOGGER.warn("Ignored fix registered for version: {} as the DataVersion of the game is: {}", version, dataVersion);
            return;
        }

        globalList.add(fix);
        fixerVersions.add(fix.getVersionKey());
    }

    public Result build() {
        final DataFixerUpper fixer = new DataFixerUpper(new Int2ObjectAVLTreeMap<>(schemas), new ArrayList<>(globalList), new IntAVLTreeSet(fixerVersions));
        return new Result(fixer);
    }

    public class Result {
        private final DataFixerUpper fixerUpper;

        public Result(final DataFixerUpper fixerUpper) {
            this.fixerUpper = fixerUpper;
        }

        public DataFixer fixer() {
            return fixerUpper;
        }

        public CompletableFuture<?> optimize(final Set<DSL.TypeReference> requiredTypes, final Executor executor) {
            final Instant started = Instant.now();
            final List<CompletableFuture<?>> doneFutures = new ArrayList<>();
            final List<CompletableFuture<?>> failFutures = new ArrayList<>();

            final Set<String> requiredTypeNames = requiredTypes.stream().map(DSL.TypeReference::typeName).collect(Collectors.toSet());

            final IntIterator iterator = fixerUpper.fixerVersions().iterator();
            while (iterator.hasNext()) {
                final int versionKey = iterator.nextInt();
                final Schema schema = schemas.get(versionKey);
                for (final String typeName : schema.types()) {
                    if (!requiredTypeNames.contains(typeName)) {
                        continue;
                    }
                    final CompletableFuture<Void> doneFuture = CompletableFuture.runAsync(() -> {
                        final Type<?> dataType = schema.getType(() -> typeName);
                        final TypeRewriteRule rule = fixerUpper.getRule(DataFixUtils.getVersion(versionKey), dataVersion);
                        dataType.rewrite(rule, DataFixerUpper.OPTIMIZATION_RULE);
                    }, executor);
                    doneFutures.add(doneFuture);

                    final CompletableFuture<?> failFuture = new CompletableFuture<>();
                    doneFuture.exceptionally(e -> {
                        failFuture.completeExceptionally(e);
                        return null;
                    });
                    failFutures.add(failFuture);
                }
            }

            final CompletableFuture<?> doneFuture = CompletableFuture.allOf(doneFutures.toArray(CompletableFuture[]::new)).thenAccept(ignored -> {
                LOGGER.info("{} Datafixer optimizations took {} milliseconds", doneFutures.size(), Duration.between(started, Instant.now()).toMillis());
            });

            final CompletableFuture<?> failFuture = CompletableFuture.anyOf(failFutures.toArray(CompletableFuture[]::new));

            return CompletableFuture.anyOf(doneFuture, failFuture);
        }
    }
}
