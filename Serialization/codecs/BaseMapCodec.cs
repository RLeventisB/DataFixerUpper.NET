// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization.codecs;

using com.google.common.collect.ImmutableMap;
using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Datafixers.util.Unit;
using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.DynamicOps;
using DataFixerUpper.Serialization.Lifecycle;
using DataFixerUpper.Serialization.MapLike;
using DataFixerUpper.Serialization.RecordBuilder;
using it.unimi.dsi.fastutil.objects.Object2ObjectArrayMap;
using it.unimi.dsi.fastutil.objects.Object2ObjectMap;

using java.util.Map;
using java.util.Optional;
using java.util.stream.Stream;

public interface BaseMapCodec<K, V> {
    Codec<K> keyCodec();

    Codec<V> elementCodec();

    default <T> DataResult<Map<K, V>> decode(final DynamicOps<T> ops, final MapLike<T> input) {
        final Object2ObjectMap<K, V> read = new Object2ObjectArrayMap<>();
        final Stream.Builder<Pair<T, T>> failed = Stream.builder();

        final DataResult<Unit> result = input.entries().reduce(
            DataResult.success(Unit.INSTANCE, Lifecycle.stable()),
            (r, pair) -> {
                final DataResult<K> key = keyCodec().parse(ops, pair.getFirst());
                final DataResult<V> value = elementCodec().parse(ops, pair.getSecond());

                final DataResult<Pair<K, V>> entryResult = key.apply2stable(Pair::of, value);
                final Optional<Pair<K, V>> entry = entryResult.resultOrPartial();
                if (entry.isPresent()) {
                    final V existingValue = read.putIfAbsent(entry.get().getFirst(), entry.get().getSecond());
                    if (existingValue != null) {
                        failed.add(pair);
                        return r.apply2stable((u, p) -> u, DataResult.error(() -> "Duplicate entry for key: '" + entry.get().getFirst() + "'"));
                    }
                }
                if (entryResult.isError()) {
                    failed.add(pair);
                }

                return r.apply2stable((u, p) -> u, entryResult);
            },
            (r1, r2) -> r1.apply2stable((u1, u2) -> u1, r2)
        );

        final Map<K, V> elements = ImmutableMap.copyOf(read);
        final T errors = ops.createMap(failed.build());

        return result.map(unit -> elements).setPartial(elements).mapError(e -> e + " missed input: " + errors);
    }

    default <T> RecordBuilder<T> encode(final Map<K, V> input, final DynamicOps<T> ops, final RecordBuilder<T> prefix) {
        for (final Map.Entry<K, V> entry : input.entrySet()) {
            prefix.add(keyCodec().encodeStart(ops, entry.getKey()), elementCodec().encodeStart(ops, entry.getValue()));
        }
        return prefix;
    }
}
