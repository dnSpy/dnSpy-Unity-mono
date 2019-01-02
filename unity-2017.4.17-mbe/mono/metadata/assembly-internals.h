/*
 * Copyright 2015 Xamarin Inc
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_ASSEMBLY_INTERNALS_H__
#define __MONO_METADATA_ASSEMBLY_INTERNALS_H__

#include <mono/metadata/assembly.h>
#include <mono/metadata/metadata-internals.h>

/* Flag bits for mono_assembly_names_equal_flags (). */
typedef enum {
		/* Default comparison: all fields must match */
	MONO_ANAME_EQ_NONE = 0x0,
		/* Don't compare public key token */
	MONO_ANAME_EQ_IGNORE_PUBKEY = 0x1,
		/* Don't compare the versions */
	MONO_ANAME_EQ_IGNORE_VERSION = 0x2,
		/* When comparing simple names, ignore case differences */
	MONO_ANAME_EQ_IGNORE_CASE = 0x4,

	MONO_ANAME_EQ_MASK = 0x7
} MonoAssemblyNameEqFlags;

gboolean
mono_assembly_names_equal_flags (MonoAssemblyName *l, MonoAssemblyName *r, MonoAssemblyNameEqFlags flags);

MONO_API MonoImage*    mono_assembly_load_module_checked (MonoAssembly *assembly, uint32_t idx, MonoError *error);

MonoAssembly * mono_assembly_open_a_lot (const char *filename, MonoImageOpenStatus *status, gboolean refonly, gboolean load_from_context);

#endif /* __MONO_METADATA_ASSEMBLY_INTERNALS_H__ */
