// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers;

using DataFixerUpper.Datafixers.schemas.Schema;
using DataFixerUpper.Serialization.Dynamic;

public interface DataFixer {
    <T> Dynamic<T> update(DSL.TypeReference type, Dynamic<T> input, int version, int newVersion);

    Schema getSchema(int key);
}
