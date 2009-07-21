/*
 * Process Hacker Driver - 
 *   internal object manager
 * 
 * Copyright (C) 2009 wj32
 * 
 * This file is part of Process Hacker.
 * 
 * Process Hacker is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Process Hacker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Process Hacker.  If not, see <http://www.gnu.org/licenses/>.
 */

#ifndef _REF_H
#define _REF_H

#include "kprocesshacker.h"

#define TAG_KPHOBJ ('bOhP')

#define KphObjectToObjectHeader(Object) ((PKPH_OBJECT_HEADER)CONTAINING_RECORD((PCHAR)(Object), KPH_OBJECT_HEADER, Body))
#define KphObjectHeaderToObject(ObjectHeader) (&((PKPH_OBJECT_HEADER)(ObjectHeader))->Body)
#define KphpAddObjectHeaderSize(Size) ((Size) + sizeof(KPH_OBJECT_HEADER) - sizeof(ULONG))

struct _KPH_OBJECT_TYPE;

/* Object flags */
#define KPHOBJ_RAISE_ON_FAIL 0x00000001
#define KPHOBJ_PAGED_POOL 0x00000002
#define KPHOBJ_NONPAGED_POOL 0x00000004
#define KPHOBJ_VALID_FLAGS 0x00000007

/* Object type callbacks */
typedef VOID (NTAPI *PKPH_TYPE_DELETE_PROCEDURE)(
    __in PVOID Object,
    __in ULONG Flags,
    __in SIZE_T Size
    );

typedef struct _KPH_OBJECT_HEADER
{
    /* The reference count of the object. */
    LONG RefCount;
    /* The flags that were used to create the object. */
    ULONG Flags;
    /* The size of the object, excluding the header. */
    SIZE_T Size;
    /* The type of the object. */
    struct _KPH_OBJECT_TYPE *Type;
    /* A linked list entry for an optional object manager object list. 
     * For example, this may be used to free all objects when the 
     * driver exits.
     */
    LIST_ENTRY GlobalObjectListEntry;
    /* A linked list entry for use by clients. For example, this may 
     * be used to dereference all objects when a driver client 
     * disconnects.
     */
    LIST_ENTRY LocalObjectListEntry;
    
    /* The body of the object. For use by the KphObject(Header)ToObject(Header) macros. */
    ULONG Body;
} KPH_OBJECT_HEADER, *PKPH_OBJECT_HEADER;

typedef struct _KPH_OBJECT_TYPE
{
    /* A fast mutex protecting the type (not used). */
    FAST_MUTEX Mutex;
    
    /* The default pool type for objects of this type, used when the 
     * pool type is not specified when an object is created. */
    POOL_TYPE DefaultPoolType;
    /* An optional procedure called when objects of this type are freed. */
    PKPH_TYPE_DELETE_PROCEDURE DeleteProcedure;
    
    /* The total number of objects of this type that are alive. */
    ULONG NumberOfObjects;
} KPH_OBJECT_TYPE, *PKPH_OBJECT_TYPE;

#ifndef _REF_PRIVATE
extern PKPH_OBJECT_TYPE KphObjectType;
#endif

NTSTATUS KphRefInit();

NTSTATUS KphRefDeinit();

NTSTATUS KphCreateObject(
    __out PVOID *Object,
    __in SIZE_T ObjectSize,
    __in ULONG Flags,
    __in_opt PKPH_OBJECT_TYPE ObjectType,
    __in_opt LONG AdditionalReferences
    );

NTSTATUS KphCreateObjectType(
    __out PKPH_OBJECT_TYPE *ObjectType,
    __in POOL_TYPE DefaultPoolType,
    __in PKPH_TYPE_DELETE_PROCEDURE DeleteProcedure
    );

BOOLEAN KphDereferenceObject(
    __in PVOID Object
    );

BOOLEAN KphDereferenceObjectEx(
    __in PVOID Object,
    __in LONG RefCount,
    __out_opt PLONG OldRefCount
    );

PKPH_OBJECT_TYPE KphGetTypeObject(
    __in PVOID Object
    );

VOID KphReferenceObject(
    __in PVOID Object
    );

VOID KphReferenceObjectEx(
    __in PVOID Object,
    __in LONG RefCount,
    __out_opt PLONG OldRefCount
    );

#endif