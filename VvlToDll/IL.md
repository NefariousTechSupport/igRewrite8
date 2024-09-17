# IL

I'm going insane. Behold the ramblings of a woman going progressivly deeper into n-sanity.

I've been looking into the IL bytecode for a few days now and I've learned that VV do not just use the instructions defined in ECMA-335. Here are the instructions I've noted.

| Bytes     | ECMA-335 Name                              | ECMA-335 differences                                           | Notes                
|-----------|--------------------------------------------|----------------------------------------------------------------|----------------------
|      0x00 | nop                                        |                                                                | 
|      0x01 | break                                      |                                                                | 
|      0x02 | ldarg.0                                    |                                                                | 
|      0x03 | ldarg.1                                    |                                                                | 
|      0x04 | ldarg.2                                    |                                                                | 
|      0x05 | ldarg.3                                    |                                                                | 
|      0x06 | ldloc.0                                    | Also corresponds to 0xF2                                       | 
|      0x07 | ldloc.1                                    | Also corresponds to 0xF3                                       | 
|      0x08 | ldloc.2                                    | Also corresponds to 0xF4                                       | 
|      0x09 | ldloc.3                                    | Also corresponds to 0xF5                                       | 
|      0x0A | stloc.0                                    |                                                                | 
|      0x0B | stloc.1                                    |                                                                | 
|      0x0C | stloc.2                                    |                                                                | 
|      0x0D | stloc.3                                    |                                                                | 
|      0x0E | ldarg.s <u8 num>                           |                                                                | 
|      0x0F | ldarga.s <u8 argNum>                       |                                                                | 
|      0x10 | starg.s <u8 num>                           |                                                                | 
|      0x11 | ldloc.s <u8 num>                           | Also corresponds to 0xF1                                       | 
|      0x12 | ldloca.s <u8 indx>                         |                                                                | This function has an if else for whether it's a value type, the code is identical on both branches
|      0x13 | stloc.s <u8 indx>                          |                                                                | 
|      0x14 | ldnull                                     |                                                                | 
|      0x15 | ldc.i4.m1                                  |                                                                | 
|      0x16 | ldc.i4.0                                   |                                                                | 
|      0x17 | ldc.i4.1                                   |                                                                | 
|      0x18 | ldc.i4.2                                   |                                                                | 
|      0x19 | ldc.i4.3                                   |                                                                | 
|      0x1A | ldc.i4.4                                   |                                                                | 
|      0x1B | ldc.i4.5                                   |                                                                | 
|      0x1C | ldc.i4.6                                   |                                                                | 
|      0x1D | ldc.i4.7                                   |                                                                | 
|      0x1E | ldc.i4.8                                   |                                                                | 
|      0x1F | ldc.i4.s <i8>                              |                                                                | 
|      0x20 | ldc.i4 <i32>                               |                                                                | 
|      0x21 | ldc.i8 <i64>                               |                                                                | 
|      0x22 | ldc.r4 <f32>                               |                                                                | 
|      0x23 | ldc.r8 <f64>                               | Removed                                                        | 
|      0x25 | dup                                        |                                                                | 
|      0x26 | pop                                        |                                                                | 
|      0x27 | jmp                                        | Removed                                                        | 
|      0x28 | call <tkn>                                 |                                                                | Deferred call, not virtual
|      0x29 | calli                                      | Removed                                                        | 
|      0x2A | ret                                        |                                                                | Bools have a special case
|      0x2B | br.s <i8 target>                           |                                                                | 
|      0x2C | brfalse.s <i8 target>                      | False counts as bit 0 being 0 instead of all 8 bits being 0    | True is still non-zero, so you can have a bool that's both true and false at the same time!
|      0x2D | brtrue.s <i8 target>                       |                                                                | 
|      0x2E | beq.s <i8 target>                          |                                                                | 
|      0x2F | bge.s <i8 target>                          |                                                                | 
|      0x30 | bgt.s <i8 target>                          |                                                                | 
|      0x31 | ble.s <i8 target>                          |                                                                | 
|      0x32 | blt.s <i8 target>                          |                                                                | 
|      0x33 | bne.un.s <i8 target>                       |                                                                | 
|      0x34 | bge.un.s <i8 target>                       |                                                                | 
|      0x35 | bgt.un.s <i8 target>                       |                                                                | 
|      0x36 | ble.un.s <i8 target>                       |                                                                | 
|      0x37 | blt.un.s <i8 target>                       |                                                                | 
|      0x38 | br <i32 target>                            |                                                                | `target` possibly always little endian
|      0x39 | brfalse <i32 target>                       | False counts as bit 0 being 0 instead of all 8 bits being 0    | `target` possibly always little endian
|      0x3A | brtrue <i32 target>                        |                                                                | `target` possibly always little endian
|      0x3B | beq <i32 target>                           |                                                                | `target` possibly always little endian
|      0x3C | bge <i32 target>                           |                                                                | `target` possibly always little endian
|      0x3D | bgt <i32 target>                           |                                                                | `target` possibly always little endian
|      0x3E | ble <i32 target>                           |                                                                | `target` possibly always little endian
|      0x3F | blt <i32 target>                           |                                                                | `target` possibly always little endian
|      0x40 | bne.un <i32 target>                        |                                                                | `target` possibly always little endian
|      0x41 | bge.un                                     | Removed                                                        | 
|      0x42 | bgt.un <i32 target>                        |                                                                | `target` possibly always little endian
|      0x43 | ble.un <i32 target>                        |                                                                | `target` possibly always little endian
|      0x44 | blt.un <i32 target>                        |                                                                | `target` possibly always little endian
|      0x45 | switch <u32 numTargets> <...i32 targets>   |                                                                | 
|      0x46 | ldind.i1                                   |                                                                | 
|      0x47 | ldind.u1                                   |                                                                | 
|      0x48 | ldind.i2                                   |                                                                | 
|      0x49 | ldind.u2                                   |                                                                | 
|      0x4A | ldind.i4                                   |                                                                | 
|      0x4B | ldind.u4                                   |                                                                | 
|      0x4C | ldind.u8/ldind.i8                          |                                                                | 
|      0x4D | ldind.i                                    |                                                                | 
|      0x4E | ldind.r4                                   |                                                                | 
|      0x4F | ldind.r8                                   | Removed                                                        | 
|      0x50 | ldind.ref                                  |                                                                | 
|      0x51 | stind.ref                                  |                                                                | 
|      0x52 | stind.i1                                   |                                                                | 
|      0x53 | stind.i2                                   |                                                                | 
|      0x54 | stind.i4                                   |                                                                | 
|      0x55 | stind.i8                                   |                                                                | 
|      0x56 | stind.r4                                   |                                                                | 
|      0x57 | stind.r8                                   | Removed                                                        | 
|      0x58 | add                                        |                                                                | Assumes second item on the stack is of the same type as the first. Does not support doubles.
|      0x59 | sub                                        |                                                                | Same as `add`
|      0x5A | mul                                        |                                                                | Same as `add`
|      0x5B | div                                        |                                                                | Same as `add`
|      0x5C | div.un                                     |                                                                | first item is unsigned, next item is signed
|      0x5D | rem                                        | both rem and rem.un deal with signed and unsigned integers     | 
|      0x5E | rem.un                                     | both rem and rem.un deal with signed and unsigned integers     | 
|      0x5F | and                                        |                                                                | 
|      0x60 | or                                         |                                                                | 
|      0x61 | xor                                        |                                                                | 
|      0x62 | shl                                        |                                                                | 
|      0x63 | shr                                        |                                                                | 
|      0x64 | shr.un                                     |                                                                | 
|      0x65 | neg                                        |                                                                | 
|      0x66 | not                                        |                                                                | 
|      0x67 | conv.i1                                    |                                                                | 
|      0x68 | conv.i2                                    |                                                                | 
|      0x69 | conv.i4                                    |                                                                | 
|      0x6A | conv.i8                                    |                                                                | 
|      0x6B | conv.r4                                    |                                                                | 
|      0x6C | conv.r8                                    | Removed                                                        | 
|      0x6D | conv.u4                                    |                                                                | 
|      0x6E | conv.u8                                    |                                                                | 
|      0x6F | callvirt <tkn>                             |                                                                | Deferred call, virtual
|      0x70 | cpobj                                      | Removed                                                        | 
|      0x71 | ldobj                                      | Removed                                                        | 
|      0x72 | ldstr <str>                                |                                                                | 
|      0x73 | newobj <ctortkn>                           |                                                                | 
|      0x74 | castclass <tkn>                            | Incorrect, identical to `isinst` for some reason.              | 
|      0x75 | isinst <tkn>                               |                                                                | 
|      0x76 | conv.r.un                                  | Removed                                                        | 
|      0x79 | unbox                                      | Removed                                                        | 
|      0x7A | throw                                      | Removed                                                        | 
|      0x7B | ldfld <tkn>                                |                                                                | 
|      0x7C | ldflda <tkn>                               |                                                                | 
|      0x7D | stfld <tkn>                                |                                                                | 
|      0x7E | ldsfld <tkn>                               |                                                                | 
|      0x7F | ldsflda <tkn>                              |                                                                | 
|      0x80 | stsfld <tkn>                               |                                                                | 
|      0x81 | stobj                                      | Removed                                                        | 
|      0x82 | conv.ovf.i1.un                             | Removed                                                        | 
|      0x83 | conv.ovf.i2.un                             | Removed                                                        | 
|      0x84 | conv.ovf.i4.un                             | Removed                                                        | 
|      0x85 | conv.ovf.i8.un                             | Removed                                                        | 
|      0x86 | conv.ovf.u1.un                             | Removed                                                        | 
|      0x87 | conv.ovf.u2.un                             | Removed                                                        | 
|      0x88 | conv.ovf.u4.un                             | Removed                                                        | 
|      0x89 | conv.ovf.u8.un                             | Removed                                                        | 
|      0x8A | conv.ovf.i.un                              | Removed                                                        | 
|      0x8B | conv.ovf.u.un                              | Removed                                                        | 
|      0x8C | box <T typeTok>                            | Effectively `nop`, the inline arg isn't even read              | 
|      0x8D | newarr <T typeTok>                         |                                                                | 
|      0x8E | ldlen                                      |                                                                | 
|      0x8F | ldelema <T typeTok>                        |                                                                | 
|      0x90 | ldelem.i1                                  |                                                                | 
|      0x91 | ldelem.u1                                  |                                                                | 
|      0x92 | ldelem.i2                                  |                                                                | 
|      0x93 | ldelem.u2                                  |                                                                | 
|      0x94 | ldelem.i4                                  |                                                                | Shares code with ldelem.i
|      0x95 | ldelem.u4                                  |                                                                | 
|      0x96 | ldelem.i8                                  |                                                                | 
|      0x97 | ldelem.i                                   |                                                                | Shares code with ldelem.i4
|      0x98 | ldelem.r4                                  |                                                                | 
|      0x99 | ldelem.r8                                  | Removed                                                        | 
|      0x9A | ldelem.ref                                 |                                                                | 
|      0x9B | stelem.i                                   |                                                                | 
|      0x9C | stelem.i1                                  |                                                                | 
|      0x9D | stelem.i2                                  |                                                                | 
|      0x9E | stelem.i4                                  |                                                                | 
|      0x9F | stelem.i8                                  |                                                                | 
|      0xA0 | stelem.r4                                  |                                                                | 
|      0xA1 | stelem.r8                                  | Removed                                                        | 
|      0xA2 | stelem.ref                                 |                                                                | 
|      0xA3 | ldelem                                     |                                                                | 
|      0xA4 | stelem                                     |                                                                | 
|      0xA5 | unbox.any <T typeTok>                      | Effectively `nop`, the inline arg isn't even read              | 
|      0xB3 | conv.ovf.i1                                | Removed                                                        | 
|      0xB4 | conv.ovf.u1                                | Removed                                                        | 
|      0xB5 | conv.ovf.i2                                | Removed                                                        | 
|      0xB6 | conv.ovf.u2                                | Removed                                                        | 
|      0xB7 | conv.ovf.i4                                | Removed                                                        | 
|      0xB8 | conv.ovf.u4                                | Removed                                                        | 
|      0xB9 | conv.ovf.i8                                | Removed                                                        | 
|      0xBA | conv.ovf.u8                                | Removed                                                        | 
|      0xC2 | refanyval                                  | Removed                                                        | 
|      0xC3 | ckfinite                                   | Removed                                                        | 
|      0xC6 | mkrefany                                   | Removed                                                        | 
|      0xD0 | ldtoken                                    |                                                                | 
|      0xD1 | conv.u2                                    |                                                                | 
|      0xD2 | conv.u1                                    |                                                                | 
|      0xD3 | conv.i                                     | Removed                                                        | 
|      0xD4 | conv.ovf.i                                 | Removed                                                        | 
|      0xD5 | conv.ovf.u                                 | Removed                                                        | 
|      0xD6 | add.ovf                                    | Removed                                                        | 
|      0xD7 | add.ovf.un                                 | Removed                                                        | 
|      0xD8 | mul.ovf                                    | Removed                                                        | 
|      0xD9 | mul.ovf.un                                 |                                                                | 
|      0xDA | sub.ovf                                    | Removed                                                        | 
|      0xDB | sub.ovf.un                                 | Removed                                                        | 
|      0xDC | endfinally                                 | Removed                                                        | 
|      0xDD | leave                                      |                                                                | 
|      0xDE | leave.s                                    |                                                                | 
|      0xDF | stind.i                                    |                                                                | 
|      0xE0 | conv.u                                     | Removed                                                        | 
|      0xF0 | ldfld <32-bit igMetaField*> ????           | This is a fully custom instruction                             | same as `ldfld` but the field reference is a 32 bit pointer?????
|      0xF1 | ldloc.s <u8 num>                           | This alias doesn't exist in ECMA-335                           | 
|      0xF2 | ldloc.0                                    | This alias doesn't exist in ECMA-335                           | 
|      0xF3 | ldloc.1                                    | This alias doesn't exist in ECMA-335                           | 
|      0xF4 | ldloc.2                                    | This alias doesn't exist in ECMA-335                           | 
|      0xF5 | ldloc.3                                    | This alias doesn't exist in ECMA-335                           | 
|      0xF6 | adddel <methodtkn>                         | This is a fully custom instruction                             | Stack transformation: `..., delegate, target, method -> ..., delegate`. Desc: Creates a delegate from the `igObject` `target` and the `DotNetMethodDefinition` `method` and combines it with the `Delegate` `delegate`. the declaring type of `methodtkn` must be the same as the method definition on the stack, the one on the stack is combined with the delegate. Also skip the next instruction
|      0xF7 | remdel <9 bytes>                           | This is a fully custom instruction                             | Stack transformation: `..., delegate, target, method -> ..., delegate`. Desc: removes the delegate matching that method and target. Also skip the next instruction
|      0xF8 | adddelinplace                              |                                                                | 
|      0xF9 | remdelinplace                              |                                                                | 
| 0xFE 0x00 | arglist                                    | Removed                                                        | 
| 0xFE 0x01 | ceq                                        |                                                                | 
| 0xFE 0x02 | cgt                                        |                                                                | 
| 0xFE 0x03 | cgt.un                                     |                                                                | 
| 0xFE 0x04 | clt                                        |                                                                | 
| 0xFE 0x05 | clt.un                                     |                                                                | 
| 0xFE 0x06 | ldftn                                      |                                                                | 
| 0xFE 0x07 | ldvirtftn                                  |                                                                | 
| 0xFE 0x09 | ldarg                                      | Removed                                                        | 
| 0xFE 0x0A | ldarga                                     | Removed                                                        | 
| 0xFE 0x0B | starg                                      |                                                                | 
| 0xFE 0x0C | ldloc                                      |                                                                | 
| 0xFE 0x0D | ldloca                                     |                                                                | 
| 0xFE 0x0E | stloc                                      |                                                                | 
| 0xFE 0x0F | localloc                                   | Removed                                                        | 
| 0xFE 0x11 | endfilter                                  | Removed                                                        | 
| 0xFE 0x12 | unaligned                                  | Removed                                                        | 
| 0xFE 0x13 | volatile                                   | Removed                                                        | 
| 0xFE 0x14 | tail                                       | Removed                                                        | 
| 0xFE 0x15 | initobj                                    | Removed                                                        | 
| 0xFE 0x16 | constrained                                | Effectively `nop`, the inline arg isn't even read              | 
| 0xFE 0x17 | cpblk                                      | Removed                                                        | 
| 0xFE 0x18 | initblk                                    | Removed                                                        | 
| 0xFE 0x19 | no                                         | Removed                                                        | 
| 0xFE 0x1A | rethrow                                    | Removed                                                        | 
| 0xFE 0x1C | sizeof                                     | Removed                                                        | 
| 0xFE 0x1D | refanytype                                 | Removed                                                        | 
| 0xFE 0x1E | readonly                                   | Removed                                                        | 
