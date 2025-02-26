// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization.codecs;

using DataFixerUpper.Datafixers.util.Either;
using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.DynamicOps;

public record EitherCodec<F, S>(Codec<F> first, Codec<S> second) implements Codec<Either<F, S>> {
    @Override
    public <T> DataResult<Pair<Either<F, S>, T>> decode(final DynamicOps<T> ops, final T input) {
        final DataResult<Pair<Either<F, S>, T>> firstRead = first.decode(ops, input).map(vo -> vo.mapFirst(Either::left));
        if (firstRead.isSuccess()) {
            return firstRead;
        }
        final DataResult<Pair<Either<F, S>, T>> secondRead = second.decode(ops, input).map(vo -> vo.mapFirst(Either::right));
        if (secondRead.isSuccess()) {
            return secondRead;
        }
        if (firstRead.hasResultOrPartial()) {
            return firstRead;
        }
        if (secondRead.hasResultOrPartial()) {
            return secondRead;
        }
        return DataResult.error(() -> "Failed to parse either. First: " + firstRead.error().orElseThrow().message() + "; Second: " + secondRead.error().orElseThrow().message());
    }

    @Override
    public <T> DataResult<T> encode(final Either<F, S> input, final DynamicOps<T> ops, final T prefix) {
        return input.map(
            value1 -> first.encode(value1, ops, prefix),
            value2 -> second.encode(value2, ops, prefix)
        );
    }
}
