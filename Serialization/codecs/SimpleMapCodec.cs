// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization.codecs;

using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.DynamicOps;
using DataFixerUpper.Serialization.Keyable;
using DataFixerUpper.Serialization.MapCodec;
using DataFixerUpper.Serialization.MapLike;
using DataFixerUpper.Serialization.RecordBuilder;

using java.util.Map;
using java.util.Objects;
using java.util.stream.Stream;

/**
 * Key and value decoded independently, statically known set of keys
 */
public final class SimpleMapCodec<K, V> extends MapCodec<Map<K, V>> implements BaseMapCodec<K, V> {
    private final Codec<K> keyCodec;
    private final Codec<V> elementCodec;
    private final Keyable keys;

    public SimpleMapCodec(final Codec<K> keyCodec, final Codec<V> elementCodec, final Keyable keys) {
        this.keyCodec = keyCodec;
        this.elementCodec = elementCodec;
        this.keys = keys;
    }

    @Override
    public Codec<K> keyCodec() {
        return keyCodec;
    }

    @Override
    public Codec<V> elementCodec() {
        return elementCodec;
    }

    @Override
    public <T> Stream<T> keys(final DynamicOps<T> ops) {
        return keys.keys(ops);
    }

    @Override
    public <T> DataResult<Map<K, V>> decode(final DynamicOps<T> ops, final MapLike<T> input) {
        return BaseMapCodec.super.decode(ops, input);
    }

    @Override
    public <T> RecordBuilder<T> encode(final Map<K, V> input, final DynamicOps<T> ops, final RecordBuilder<T> prefix) {
        return BaseMapCodec.super.encode(input, ops, prefix);
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || getClass() != o.getClass()) {
            return false;
        }
        final SimpleMapCodec<?, ?> that = (SimpleMapCodec<?, ?>) o;
        return Objects.equals(keyCodec, that.keyCodec) && Objects.equals(elementCodec, that.elementCodec);
    }

    @Override
    public int hashCode() {
        return Objects.hash(keyCodec, elementCodec);
    }

    @Override
    public String toString() {
        return "SimpleMapCodec[" + keyCodec + " -> " + elementCodec + ']';
    }
}
