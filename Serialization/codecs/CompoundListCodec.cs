// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization.codecs;

using com.google.common.collect.ImmutableList;
using com.google.common.collect.ImmutableMap;
using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Datafixers.util.Unit;
using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.DynamicOps;
using DataFixerUpper.Serialization.Lifecycle;
using DataFixerUpper.Serialization.RecordBuilder;

using java.util.List;
using java.util.Objects;
using java.util.concurrent.atomic.AtomicReference;

public final class CompoundListCodec<K, V> implements Codec<List<Pair<K, V>>> {
    private final Codec<K> keyCodec;
    private final Codec<V> elementCodec;

    public CompoundListCodec(final Codec<K> keyCodec, final Codec<V> elementCodec) {
        this.keyCodec = keyCodec;
        this.elementCodec = elementCodec;
    }

    @Override
    public <T> DataResult<Pair<List<Pair<K, V>>, T>> decode(final DynamicOps<T> ops, final T input) {
        return ops.getMapEntries(input).flatMap(map -> {
            final ImmutableList.Builder<Pair<K, V>> read = ImmutableList.builder();
            final ImmutableMap.Builder<T, T> failed = ImmutableMap.builder();

            final AtomicReference<DataResult<Unit>> result = new AtomicReference<>(DataResult.success(Unit.INSTANCE, Lifecycle.experimental()));

            map.accept((key, value) -> {
                final DataResult<K> k = keyCodec.parse(ops, key);
                final DataResult<V> v = elementCodec.parse(ops, value);

                final DataResult<Pair<K, V>> readEntry = k.apply2stable(Pair::new, v);

                readEntry.error().ifPresent(e -> failed.put(key, value));

                result.setPlain(result.getPlain().apply2stable((u, e) -> {
                    read.add(e);
                    return u;
                }, readEntry));
            });

            final ImmutableList<Pair<K, V>> elements = read.build();
            final T errors = ops.createMap(failed.build());

            final Pair<List<Pair<K, V>>, T> pair = Pair.of(elements, errors);

            return result.getPlain().map(unit -> pair).setPartial(pair);
        });
    }

    @Override
    public <T> DataResult<T> encode(final List<Pair<K, V>> input, final DynamicOps<T> ops, final T prefix) {
        final RecordBuilder<T> builder = ops.mapBuilder();

        for (final Pair<K, V> pair : input) {
            builder.add(keyCodec.encodeStart(ops, pair.getFirst()), elementCodec.encodeStart(ops, pair.getSecond()));
        }

        return builder.build(prefix);
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || getClass() != o.getClass()) {
            return false;
        }
        final CompoundListCodec<?, ?> that = (CompoundListCodec<?, ?>) o;
        return Objects.equals(keyCodec, that.keyCodec) && Objects.equals(elementCodec, that.elementCodec);
    }

    @Override
    public int hashCode() {
        return Objects.hash(keyCodec, elementCodec);
    }

    @Override
    public String toString() {
        return "CompoundListCodec[" + keyCodec + " -> " + elementCodec + ']';
    }
}
