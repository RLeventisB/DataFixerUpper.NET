// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Serialization;

public interface Compressable extends Keyable {
    <T> KeyCompressor<T> compressor(final DynamicOps<T> ops);
}
