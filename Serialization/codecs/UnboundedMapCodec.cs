// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization.codecs;

using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.DynamicOps;
using DataFixerUpper.Serialization.Lifecycle;

using java.util.Map;

/**
 * Key and value decoded independently, unknown set of keys
 */
public record UnboundedMapCodec<K, V>(
    Codec<K> keyCodec,
    Codec<V> elementCodec
) implements BaseMapCodec<K, V>, Codec<Map<K, V>> {
    @Override
    public <T> DataResult<Pair<Map<K, V>, T>> decode(final DynamicOps<T> ops, final T input) {
        return ops.getMap(input).setLifecycle(Lifecycle.stable()).flatMap(map -> decode(ops, map)).map(r -> Pair.of(r, input));
    }

    @Override
    public <T> DataResult<T> encode(final Map<K, V> input, final DynamicOps<T> ops, final T prefix) {
        return encode(input, ops, ops.mapBuilder()).build(prefix);
    }

    @Override
    public String toString() {
        return "UnboundedMapCodec[" + keyCodec + " -> " + elementCodec + ']';
    }
}
