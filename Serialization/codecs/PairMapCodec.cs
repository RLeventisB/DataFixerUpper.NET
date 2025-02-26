// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization.codecs;

using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.DynamicOps;
using DataFixerUpper.Serialization.MapCodec;
using DataFixerUpper.Serialization.MapLike;
using DataFixerUpper.Serialization.RecordBuilder;

using java.util.Objects;
using java.util.stream.Stream;

public final class PairMapCodec<F, S> extends MapCodec<Pair<F, S>> {
    private final MapCodec<F> first;
    private final MapCodec<S> second;

    public PairMapCodec(final MapCodec<F> first, final MapCodec<S> second) {
        this.first = first;
        this.second = second;
    }

    @Override
    public <T> DataResult<Pair<F, S>> decode(final DynamicOps<T> ops, final MapLike<T> input) {
        return first.decode(ops, input).flatMap(p1 ->
            second.decode(ops, input).map(p2 ->
                Pair.of(p1, p2)
            )
        );
    }

    @Override
    public <T> RecordBuilder<T> encode(final Pair<F, S> input, final DynamicOps<T> ops, final RecordBuilder<T> prefix) {
        return first.encode(input.getFirst(), ops, second.encode(input.getSecond(), ops, prefix));
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || getClass() != o.getClass()) {
            return false;
        }
        final PairMapCodec<?, ?> pairCodec = (PairMapCodec<?, ?>) o;
        return Objects.equals(first, pairCodec.first) && Objects.equals(second, pairCodec.second);
    }

    @Override
    public int hashCode() {
        return Objects.hash(first, second);
    }

    @Override
    public String toString() {
        return "PairMapCodec[" + first + ", " + second + ']';
    }

    @Override
    public <T> Stream<T> keys(final DynamicOps<T> ops) {
        return Stream.concat(first.keys(ops), second.keys(ops));
    }
}
