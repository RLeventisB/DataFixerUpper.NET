// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization;

using DataFixerUpper.Datafixers.util.Pair;

using javax.annotation.Nullable;
using java.util.Map;
using java.util.stream.Stream;

public interface MapLike<T> {
    @Nullable
    T get(final T key);

    @Nullable
    T get(final String key);

    Stream<Pair<T, T>> entries();

    static <T> MapLike<T> forMap(final Map<T, T> map, final DynamicOps<T> ops) {
        return new MapLike<T>() {
            @Nullable
            @Override
            public T get(final T key) {
                return map.get(key);
            }

            @Nullable
            @Override
            public T get(final String key) {
                return get(ops.createString(key));
            }

            @Override
            public Stream<Pair<T, T>> entries() {
                return map.entrySet().stream().map(e -> Pair.of(e.getKey(), e.getValue()));
            }

            @Override
            public String toString() {
                return "MapLike[" + map + "]";
            }
        };
    }
}
