/*
 * Copyright 2018-2020 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .data

  .globl _libcuda_so_tramp_table
  .hidden _libcuda_so_tramp_table
  .align 8
_libcuda_so_tramp_table:
  .zero 3840

  .text

  .globl _libcuda_so_tramp_resolve
  .hidden _libcuda_so_tramp_resolve

  .globl _libcuda_so_save_regs_and_resolve
  .hidden _libcuda_so_save_regs_and_resolve
  .type _libcuda_so_save_regs_and_resolve, %function
_libcuda_so_save_regs_and_resolve:
  .cfi_startproc

#define PUSH_REG(reg) pushq %reg ; .cfi_adjust_cfa_offset 8; .cfi_rel_offset reg, 0
#define POP_REG(reg) popq %reg ; .cfi_adjust_cfa_offset -8; .cfi_restore reg

#define DEC_STACK(d) subq $d, %rsp; .cfi_adjust_cfa_offset d
#define INC_STACK(d) addq $d, %rsp; .cfi_adjust_cfa_offset -d

#define PUSH_XMM_REG(reg) DEC_STACK(16); movdqa %reg, (%rsp); .cfi_rel_offset reg, 0
#define POP_XMM_REG(reg) movdqa (%rsp), %reg; .cfi_restore reg; INC_STACK(16)

  // Slow path which calls dlsym, taken only on first call.
  // All registers are stored to handle arbitrary calling conventions
  // (except x87 FPU registers which do not have to be preserved).
  // For Dwarf directives, read https://www.imperialviolet.org/2017/01/18/cfi.html.

  // FIXME: AVX (YMM, ZMM) registers are NOT saved to simplify code.

  PUSH_REG(rdi)  // 16
  mov 0x10(%rsp), %rdi
  PUSH_REG(rax)
  PUSH_REG(rbx)  // 16
  PUSH_REG(rcx)
  PUSH_REG(rdx)  // 16
  PUSH_REG(rbp)
  PUSH_REG(rsi)  // 16
  PUSH_REG(r8)
  PUSH_REG(r9)  // 16
  PUSH_REG(r10)
  PUSH_REG(r11)  // 16
  PUSH_REG(r12)
  PUSH_REG(r13)  // 16
  PUSH_REG(r14)
  PUSH_REG(r15)  // 16
  PUSH_XMM_REG(xmm0)
  PUSH_XMM_REG(xmm1)
  PUSH_XMM_REG(xmm2)
  PUSH_XMM_REG(xmm3)
  PUSH_XMM_REG(xmm4)
  PUSH_XMM_REG(xmm5)
  PUSH_XMM_REG(xmm6)
  PUSH_XMM_REG(xmm7)

  // Stack is just 8-byte aligned but callee will re-align to 16
  call _libcuda_so_tramp_resolve

  POP_XMM_REG(xmm7)
  POP_XMM_REG(xmm6)
  POP_XMM_REG(xmm5)
  POP_XMM_REG(xmm4)
  POP_XMM_REG(xmm3)
  POP_XMM_REG(xmm2)
  POP_XMM_REG(xmm1)
  POP_XMM_REG(xmm0)  // 16
  POP_REG(r15)
  POP_REG(r14)  // 16
  POP_REG(r13)
  POP_REG(r12)  // 16
  POP_REG(r11)
  POP_REG(r10)  // 16
  POP_REG(r9)
  POP_REG(r8)  // 16
  POP_REG(rsi)
  POP_REG(rbp)  // 16
  POP_REG(rdx)
  POP_REG(rcx)  // 16
  POP_REG(rbx)
  POP_REG(rax)  // 16
  POP_REG(rdi)

  ret

  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArray3DCreate
  .p2align 4
  .type cuArray3DCreate, %function
cuArray3DCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+0(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+0(%rip)
2:
  pushq $0
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArray3DCreate_v2
  .p2align 4
  .type cuArray3DCreate_v2, %function
cuArray3DCreate_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+8(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+8(%rip)
2:
  pushq $1
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArray3DGetDescriptor
  .p2align 4
  .type cuArray3DGetDescriptor, %function
cuArray3DGetDescriptor:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+16(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+16(%rip)
2:
  pushq $2
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArray3DGetDescriptor_v2
  .p2align 4
  .type cuArray3DGetDescriptor_v2, %function
cuArray3DGetDescriptor_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+24(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+24(%rip)
2:
  pushq $3
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArrayCreate
  .p2align 4
  .type cuArrayCreate, %function
cuArrayCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+32(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+32(%rip)
2:
  pushq $4
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArrayCreate_v2
  .p2align 4
  .type cuArrayCreate_v2, %function
cuArrayCreate_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+40(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+40(%rip)
2:
  pushq $5
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArrayDestroy
  .p2align 4
  .type cuArrayDestroy, %function
cuArrayDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+48(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+48(%rip)
2:
  pushq $6
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArrayGetDescriptor
  .p2align 4
  .type cuArrayGetDescriptor, %function
cuArrayGetDescriptor:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+56(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+56(%rip)
2:
  pushq $7
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArrayGetDescriptor_v2
  .p2align 4
  .type cuArrayGetDescriptor_v2, %function
cuArrayGetDescriptor_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+64(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+64(%rip)
2:
  pushq $8
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuArrayGetSparseProperties
  .p2align 4
  .type cuArrayGetSparseProperties, %function
cuArrayGetSparseProperties:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+72(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+72(%rip)
2:
  pushq $9
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxAttach
  .p2align 4
  .type cuCtxAttach, %function
cuCtxAttach:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+80(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+80(%rip)
2:
  pushq $10
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxCreate
  .p2align 4
  .type cuCtxCreate, %function
cuCtxCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+88(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+88(%rip)
2:
  pushq $11
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxCreate_v2
  .p2align 4
  .type cuCtxCreate_v2, %function
cuCtxCreate_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+96(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+96(%rip)
2:
  pushq $12
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxDestroy
  .p2align 4
  .type cuCtxDestroy, %function
cuCtxDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+104(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+104(%rip)
2:
  pushq $13
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxDestroy_v2
  .p2align 4
  .type cuCtxDestroy_v2, %function
cuCtxDestroy_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+112(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+112(%rip)
2:
  pushq $14
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxDetach
  .p2align 4
  .type cuCtxDetach, %function
cuCtxDetach:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+120(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+120(%rip)
2:
  pushq $15
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxDisablePeerAccess
  .p2align 4
  .type cuCtxDisablePeerAccess, %function
cuCtxDisablePeerAccess:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+128(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+128(%rip)
2:
  pushq $16
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxEnablePeerAccess
  .p2align 4
  .type cuCtxEnablePeerAccess, %function
cuCtxEnablePeerAccess:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+136(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+136(%rip)
2:
  pushq $17
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetApiVersion
  .p2align 4
  .type cuCtxGetApiVersion, %function
cuCtxGetApiVersion:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+144(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+144(%rip)
2:
  pushq $18
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetCacheConfig
  .p2align 4
  .type cuCtxGetCacheConfig, %function
cuCtxGetCacheConfig:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+152(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+152(%rip)
2:
  pushq $19
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetCurrent
  .p2align 4
  .type cuCtxGetCurrent, %function
cuCtxGetCurrent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+160(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+160(%rip)
2:
  pushq $20
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetDevice
  .p2align 4
  .type cuCtxGetDevice, %function
cuCtxGetDevice:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+168(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+168(%rip)
2:
  pushq $21
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetFlags
  .p2align 4
  .type cuCtxGetFlags, %function
cuCtxGetFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+176(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+176(%rip)
2:
  pushq $22
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetLimit
  .p2align 4
  .type cuCtxGetLimit, %function
cuCtxGetLimit:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+184(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+184(%rip)
2:
  pushq $23
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetSharedMemConfig
  .p2align 4
  .type cuCtxGetSharedMemConfig, %function
cuCtxGetSharedMemConfig:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+192(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+192(%rip)
2:
  pushq $24
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxGetStreamPriorityRange
  .p2align 4
  .type cuCtxGetStreamPriorityRange, %function
cuCtxGetStreamPriorityRange:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+200(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+200(%rip)
2:
  pushq $25
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxPopCurrent
  .p2align 4
  .type cuCtxPopCurrent, %function
cuCtxPopCurrent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+208(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+208(%rip)
2:
  pushq $26
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxPopCurrent_v2
  .p2align 4
  .type cuCtxPopCurrent_v2, %function
cuCtxPopCurrent_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+216(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+216(%rip)
2:
  pushq $27
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxPushCurrent
  .p2align 4
  .type cuCtxPushCurrent, %function
cuCtxPushCurrent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+224(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+224(%rip)
2:
  pushq $28
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxPushCurrent_v2
  .p2align 4
  .type cuCtxPushCurrent_v2, %function
cuCtxPushCurrent_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+232(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+232(%rip)
2:
  pushq $29
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxResetPersistingL2Cache
  .p2align 4
  .type cuCtxResetPersistingL2Cache, %function
cuCtxResetPersistingL2Cache:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+240(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+240(%rip)
2:
  pushq $30
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxSetCacheConfig
  .p2align 4
  .type cuCtxSetCacheConfig, %function
cuCtxSetCacheConfig:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+248(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+248(%rip)
2:
  pushq $31
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxSetCurrent
  .p2align 4
  .type cuCtxSetCurrent, %function
cuCtxSetCurrent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+256(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+256(%rip)
2:
  pushq $32
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxSetLimit
  .p2align 4
  .type cuCtxSetLimit, %function
cuCtxSetLimit:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+264(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+264(%rip)
2:
  pushq $33
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxSetSharedMemConfig
  .p2align 4
  .type cuCtxSetSharedMemConfig, %function
cuCtxSetSharedMemConfig:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+272(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+272(%rip)
2:
  pushq $34
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuCtxSynchronize
  .p2align 4
  .type cuCtxSynchronize, %function
cuCtxSynchronize:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+280(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+280(%rip)
2:
  pushq $35
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDestroyExternalMemory
  .p2align 4
  .type cuDestroyExternalMemory, %function
cuDestroyExternalMemory:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+288(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+288(%rip)
2:
  pushq $36
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDestroyExternalSemaphore
  .p2align 4
  .type cuDestroyExternalSemaphore, %function
cuDestroyExternalSemaphore:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+296(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+296(%rip)
2:
  pushq $37
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceCanAccessPeer
  .p2align 4
  .type cuDeviceCanAccessPeer, %function
cuDeviceCanAccessPeer:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+304(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+304(%rip)
2:
  pushq $38
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceComputeCapability
  .p2align 4
  .type cuDeviceComputeCapability, %function
cuDeviceComputeCapability:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+312(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+312(%rip)
2:
  pushq $39
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGet
  .p2align 4
  .type cuDeviceGet, %function
cuDeviceGet:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+320(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+320(%rip)
2:
  pushq $40
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetAttribute
  .p2align 4
  .type cuDeviceGetAttribute, %function
cuDeviceGetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+328(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+328(%rip)
2:
  pushq $41
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetByPCIBusId
  .p2align 4
  .type cuDeviceGetByPCIBusId, %function
cuDeviceGetByPCIBusId:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+336(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+336(%rip)
2:
  pushq $42
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetCount
  .p2align 4
  .type cuDeviceGetCount, %function
cuDeviceGetCount:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+344(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+344(%rip)
2:
  pushq $43
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetLuid
  .p2align 4
  .type cuDeviceGetLuid, %function
cuDeviceGetLuid:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+352(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+352(%rip)
2:
  pushq $44
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetName
  .p2align 4
  .type cuDeviceGetName, %function
cuDeviceGetName:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+360(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+360(%rip)
2:
  pushq $45
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetNvSciSyncAttributes
  .p2align 4
  .type cuDeviceGetNvSciSyncAttributes, %function
cuDeviceGetNvSciSyncAttributes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+368(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+368(%rip)
2:
  pushq $46
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetP2PAttribute
  .p2align 4
  .type cuDeviceGetP2PAttribute, %function
cuDeviceGetP2PAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+376(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+376(%rip)
2:
  pushq $47
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetPCIBusId
  .p2align 4
  .type cuDeviceGetPCIBusId, %function
cuDeviceGetPCIBusId:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+384(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+384(%rip)
2:
  pushq $48
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetProperties
  .p2align 4
  .type cuDeviceGetProperties, %function
cuDeviceGetProperties:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+392(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+392(%rip)
2:
  pushq $49
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetTexture1DLinearMaxWidth
  .p2align 4
  .type cuDeviceGetTexture1DLinearMaxWidth, %function
cuDeviceGetTexture1DLinearMaxWidth:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+400(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+400(%rip)
2:
  pushq $50
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceGetUuid
  .p2align 4
  .type cuDeviceGetUuid, %function
cuDeviceGetUuid:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+408(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+408(%rip)
2:
  pushq $51
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxGetState
  .p2align 4
  .type cuDevicePrimaryCtxGetState, %function
cuDevicePrimaryCtxGetState:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+416(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+416(%rip)
2:
  pushq $52
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxRelease
  .p2align 4
  .type cuDevicePrimaryCtxRelease, %function
cuDevicePrimaryCtxRelease:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+424(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+424(%rip)
2:
  pushq $53
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxRelease_v2
  .p2align 4
  .type cuDevicePrimaryCtxRelease_v2, %function
cuDevicePrimaryCtxRelease_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+432(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+432(%rip)
2:
  pushq $54
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxReset
  .p2align 4
  .type cuDevicePrimaryCtxReset, %function
cuDevicePrimaryCtxReset:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+440(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+440(%rip)
2:
  pushq $55
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxReset_v2
  .p2align 4
  .type cuDevicePrimaryCtxReset_v2, %function
cuDevicePrimaryCtxReset_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+448(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+448(%rip)
2:
  pushq $56
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxRetain
  .p2align 4
  .type cuDevicePrimaryCtxRetain, %function
cuDevicePrimaryCtxRetain:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+456(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+456(%rip)
2:
  pushq $57
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxSetFlags
  .p2align 4
  .type cuDevicePrimaryCtxSetFlags, %function
cuDevicePrimaryCtxSetFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+464(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+464(%rip)
2:
  pushq $58
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDevicePrimaryCtxSetFlags_v2
  .p2align 4
  .type cuDevicePrimaryCtxSetFlags_v2, %function
cuDevicePrimaryCtxSetFlags_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+472(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+472(%rip)
2:
  pushq $59
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceTotalMem
  .p2align 4
  .type cuDeviceTotalMem, %function
cuDeviceTotalMem:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+480(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+480(%rip)
2:
  pushq $60
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDeviceTotalMem_v2
  .p2align 4
  .type cuDeviceTotalMem_v2, %function
cuDeviceTotalMem_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+488(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+488(%rip)
2:
  pushq $61
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuDriverGetVersion
  .p2align 4
  .type cuDriverGetVersion, %function
cuDriverGetVersion:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+496(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+496(%rip)
2:
  pushq $62
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLApiInit
  .p2align 4
  .type cuEGLApiInit, %function
cuEGLApiInit:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+504(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+504(%rip)
2:
  pushq $63
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamConsumerAcquireFrame
  .p2align 4
  .type cuEGLStreamConsumerAcquireFrame, %function
cuEGLStreamConsumerAcquireFrame:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+512(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+512(%rip)
2:
  pushq $64
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamConsumerConnect
  .p2align 4
  .type cuEGLStreamConsumerConnect, %function
cuEGLStreamConsumerConnect:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+520(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+520(%rip)
2:
  pushq $65
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamConsumerConnectWithFlags
  .p2align 4
  .type cuEGLStreamConsumerConnectWithFlags, %function
cuEGLStreamConsumerConnectWithFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+528(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+528(%rip)
2:
  pushq $66
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamConsumerDisconnect
  .p2align 4
  .type cuEGLStreamConsumerDisconnect, %function
cuEGLStreamConsumerDisconnect:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+536(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+536(%rip)
2:
  pushq $67
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamConsumerReleaseFrame
  .p2align 4
  .type cuEGLStreamConsumerReleaseFrame, %function
cuEGLStreamConsumerReleaseFrame:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+544(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+544(%rip)
2:
  pushq $68
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamProducerConnect
  .p2align 4
  .type cuEGLStreamProducerConnect, %function
cuEGLStreamProducerConnect:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+552(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+552(%rip)
2:
  pushq $69
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamProducerDisconnect
  .p2align 4
  .type cuEGLStreamProducerDisconnect, %function
cuEGLStreamProducerDisconnect:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+560(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+560(%rip)
2:
  pushq $70
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamProducerPresentFrame
  .p2align 4
  .type cuEGLStreamProducerPresentFrame, %function
cuEGLStreamProducerPresentFrame:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+568(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+568(%rip)
2:
  pushq $71
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEGLStreamProducerReturnFrame
  .p2align 4
  .type cuEGLStreamProducerReturnFrame, %function
cuEGLStreamProducerReturnFrame:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+576(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+576(%rip)
2:
  pushq $72
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventCreate
  .p2align 4
  .type cuEventCreate, %function
cuEventCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+584(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+584(%rip)
2:
  pushq $73
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventDestroy
  .p2align 4
  .type cuEventDestroy, %function
cuEventDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+592(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+592(%rip)
2:
  pushq $74
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventDestroy_v2
  .p2align 4
  .type cuEventDestroy_v2, %function
cuEventDestroy_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+600(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+600(%rip)
2:
  pushq $75
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventElapsedTime
  .p2align 4
  .type cuEventElapsedTime, %function
cuEventElapsedTime:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+608(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+608(%rip)
2:
  pushq $76
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventQuery
  .p2align 4
  .type cuEventQuery, %function
cuEventQuery:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+616(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+616(%rip)
2:
  pushq $77
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventRecord
  .p2align 4
  .type cuEventRecord, %function
cuEventRecord:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+624(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+624(%rip)
2:
  pushq $78
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventRecordWithFlags
  .p2align 4
  .type cuEventRecordWithFlags, %function
cuEventRecordWithFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+632(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+632(%rip)
2:
  pushq $79
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventRecordWithFlags_ptsz
  .p2align 4
  .type cuEventRecordWithFlags_ptsz, %function
cuEventRecordWithFlags_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+640(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+640(%rip)
2:
  pushq $80
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventRecord_ptsz
  .p2align 4
  .type cuEventRecord_ptsz, %function
cuEventRecord_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+648(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+648(%rip)
2:
  pushq $81
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuEventSynchronize
  .p2align 4
  .type cuEventSynchronize, %function
cuEventSynchronize:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+656(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+656(%rip)
2:
  pushq $82
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuExternalMemoryGetMappedBuffer
  .p2align 4
  .type cuExternalMemoryGetMappedBuffer, %function
cuExternalMemoryGetMappedBuffer:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+664(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+664(%rip)
2:
  pushq $83
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuExternalMemoryGetMappedMipmappedArray
  .p2align 4
  .type cuExternalMemoryGetMappedMipmappedArray, %function
cuExternalMemoryGetMappedMipmappedArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+672(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+672(%rip)
2:
  pushq $84
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuFuncGetAttribute
  .p2align 4
  .type cuFuncGetAttribute, %function
cuFuncGetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+680(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+680(%rip)
2:
  pushq $85
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuFuncGetModule
  .p2align 4
  .type cuFuncGetModule, %function
cuFuncGetModule:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+688(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+688(%rip)
2:
  pushq $86
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuFuncSetAttribute
  .p2align 4
  .type cuFuncSetAttribute, %function
cuFuncSetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+696(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+696(%rip)
2:
  pushq $87
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuFuncSetBlockShape
  .p2align 4
  .type cuFuncSetBlockShape, %function
cuFuncSetBlockShape:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+704(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+704(%rip)
2:
  pushq $88
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuFuncSetCacheConfig
  .p2align 4
  .type cuFuncSetCacheConfig, %function
cuFuncSetCacheConfig:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+712(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+712(%rip)
2:
  pushq $89
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuFuncSetSharedMemConfig
  .p2align 4
  .type cuFuncSetSharedMemConfig, %function
cuFuncSetSharedMemConfig:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+720(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+720(%rip)
2:
  pushq $90
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuFuncSetSharedSize
  .p2align 4
  .type cuFuncSetSharedSize, %function
cuFuncSetSharedSize:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+728(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+728(%rip)
2:
  pushq $91
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLCtxCreate
  .p2align 4
  .type cuGLCtxCreate, %function
cuGLCtxCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+736(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+736(%rip)
2:
  pushq $92
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLCtxCreate_v2
  .p2align 4
  .type cuGLCtxCreate_v2, %function
cuGLCtxCreate_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+744(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+744(%rip)
2:
  pushq $93
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLGetDevices
  .p2align 4
  .type cuGLGetDevices, %function
cuGLGetDevices:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+752(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+752(%rip)
2:
  pushq $94
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLGetDevices_v2
  .p2align 4
  .type cuGLGetDevices_v2, %function
cuGLGetDevices_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+760(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+760(%rip)
2:
  pushq $95
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLInit
  .p2align 4
  .type cuGLInit, %function
cuGLInit:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+768(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+768(%rip)
2:
  pushq $96
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLMapBufferObject
  .p2align 4
  .type cuGLMapBufferObject, %function
cuGLMapBufferObject:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+776(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+776(%rip)
2:
  pushq $97
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLMapBufferObjectAsync
  .p2align 4
  .type cuGLMapBufferObjectAsync, %function
cuGLMapBufferObjectAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+784(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+784(%rip)
2:
  pushq $98
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLMapBufferObjectAsync_v2
  .p2align 4
  .type cuGLMapBufferObjectAsync_v2, %function
cuGLMapBufferObjectAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+792(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+792(%rip)
2:
  pushq $99
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLMapBufferObjectAsync_v2_ptsz
  .p2align 4
  .type cuGLMapBufferObjectAsync_v2_ptsz, %function
cuGLMapBufferObjectAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+800(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+800(%rip)
2:
  pushq $100
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLMapBufferObject_v2
  .p2align 4
  .type cuGLMapBufferObject_v2, %function
cuGLMapBufferObject_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+808(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+808(%rip)
2:
  pushq $101
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLMapBufferObject_v2_ptds
  .p2align 4
  .type cuGLMapBufferObject_v2_ptds, %function
cuGLMapBufferObject_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+816(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+816(%rip)
2:
  pushq $102
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLRegisterBufferObject
  .p2align 4
  .type cuGLRegisterBufferObject, %function
cuGLRegisterBufferObject:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+824(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+824(%rip)
2:
  pushq $103
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLSetBufferObjectMapFlags
  .p2align 4
  .type cuGLSetBufferObjectMapFlags, %function
cuGLSetBufferObjectMapFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+832(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+832(%rip)
2:
  pushq $104
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLUnmapBufferObject
  .p2align 4
  .type cuGLUnmapBufferObject, %function
cuGLUnmapBufferObject:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+840(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+840(%rip)
2:
  pushq $105
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLUnmapBufferObjectAsync
  .p2align 4
  .type cuGLUnmapBufferObjectAsync, %function
cuGLUnmapBufferObjectAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+848(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+848(%rip)
2:
  pushq $106
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGLUnregisterBufferObject
  .p2align 4
  .type cuGLUnregisterBufferObject, %function
cuGLUnregisterBufferObject:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+856(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+856(%rip)
2:
  pushq $107
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGetErrorName
  .p2align 4
  .type cuGetErrorName, %function
cuGetErrorName:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+864(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+864(%rip)
2:
  pushq $108
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGetErrorString
  .p2align 4
  .type cuGetErrorString, %function
cuGetErrorString:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+872(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+872(%rip)
2:
  pushq $109
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGetExportTable
  .p2align 4
  .type cuGetExportTable, %function
cuGetExportTable:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+880(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+880(%rip)
2:
  pushq $110
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddChildGraphNode
  .p2align 4
  .type cuGraphAddChildGraphNode, %function
cuGraphAddChildGraphNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+888(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+888(%rip)
2:
  pushq $111
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddDependencies
  .p2align 4
  .type cuGraphAddDependencies, %function
cuGraphAddDependencies:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+896(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+896(%rip)
2:
  pushq $112
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddEmptyNode
  .p2align 4
  .type cuGraphAddEmptyNode, %function
cuGraphAddEmptyNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+904(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+904(%rip)
2:
  pushq $113
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddEventRecordNode
  .p2align 4
  .type cuGraphAddEventRecordNode, %function
cuGraphAddEventRecordNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+912(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+912(%rip)
2:
  pushq $114
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddEventWaitNode
  .p2align 4
  .type cuGraphAddEventWaitNode, %function
cuGraphAddEventWaitNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+920(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+920(%rip)
2:
  pushq $115
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddHostNode
  .p2align 4
  .type cuGraphAddHostNode, %function
cuGraphAddHostNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+928(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+928(%rip)
2:
  pushq $116
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddKernelNode
  .p2align 4
  .type cuGraphAddKernelNode, %function
cuGraphAddKernelNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+936(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+936(%rip)
2:
  pushq $117
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddMemcpyNode
  .p2align 4
  .type cuGraphAddMemcpyNode, %function
cuGraphAddMemcpyNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+944(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+944(%rip)
2:
  pushq $118
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphAddMemsetNode
  .p2align 4
  .type cuGraphAddMemsetNode, %function
cuGraphAddMemsetNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+952(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+952(%rip)
2:
  pushq $119
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphChildGraphNodeGetGraph
  .p2align 4
  .type cuGraphChildGraphNodeGetGraph, %function
cuGraphChildGraphNodeGetGraph:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+960(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+960(%rip)
2:
  pushq $120
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphClone
  .p2align 4
  .type cuGraphClone, %function
cuGraphClone:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+968(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+968(%rip)
2:
  pushq $121
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphCreate
  .p2align 4
  .type cuGraphCreate, %function
cuGraphCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+976(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+976(%rip)
2:
  pushq $122
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphDestroy
  .p2align 4
  .type cuGraphDestroy, %function
cuGraphDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+984(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+984(%rip)
2:
  pushq $123
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphDestroyNode
  .p2align 4
  .type cuGraphDestroyNode, %function
cuGraphDestroyNode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+992(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+992(%rip)
2:
  pushq $124
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphEventRecordNodeGetEvent
  .p2align 4
  .type cuGraphEventRecordNodeGetEvent, %function
cuGraphEventRecordNodeGetEvent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1000(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1000(%rip)
2:
  pushq $125
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphEventRecordNodeSetEvent
  .p2align 4
  .type cuGraphEventRecordNodeSetEvent, %function
cuGraphEventRecordNodeSetEvent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1008(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1008(%rip)
2:
  pushq $126
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphEventWaitNodeGetEvent
  .p2align 4
  .type cuGraphEventWaitNodeGetEvent, %function
cuGraphEventWaitNodeGetEvent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1016(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1016(%rip)
2:
  pushq $127
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphEventWaitNodeSetEvent
  .p2align 4
  .type cuGraphEventWaitNodeSetEvent, %function
cuGraphEventWaitNodeSetEvent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1024(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1024(%rip)
2:
  pushq $128
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecChildGraphNodeSetParams
  .p2align 4
  .type cuGraphExecChildGraphNodeSetParams, %function
cuGraphExecChildGraphNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1032(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1032(%rip)
2:
  pushq $129
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecDestroy
  .p2align 4
  .type cuGraphExecDestroy, %function
cuGraphExecDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1040(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1040(%rip)
2:
  pushq $130
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecEventRecordNodeSetEvent
  .p2align 4
  .type cuGraphExecEventRecordNodeSetEvent, %function
cuGraphExecEventRecordNodeSetEvent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1048(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1048(%rip)
2:
  pushq $131
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecEventWaitNodeSetEvent
  .p2align 4
  .type cuGraphExecEventWaitNodeSetEvent, %function
cuGraphExecEventWaitNodeSetEvent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1056(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1056(%rip)
2:
  pushq $132
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecHostNodeSetParams
  .p2align 4
  .type cuGraphExecHostNodeSetParams, %function
cuGraphExecHostNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1064(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1064(%rip)
2:
  pushq $133
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecKernelNodeSetParams
  .p2align 4
  .type cuGraphExecKernelNodeSetParams, %function
cuGraphExecKernelNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1072(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1072(%rip)
2:
  pushq $134
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecMemcpyNodeSetParams
  .p2align 4
  .type cuGraphExecMemcpyNodeSetParams, %function
cuGraphExecMemcpyNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1080(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1080(%rip)
2:
  pushq $135
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecMemsetNodeSetParams
  .p2align 4
  .type cuGraphExecMemsetNodeSetParams, %function
cuGraphExecMemsetNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1088(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1088(%rip)
2:
  pushq $136
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphExecUpdate
  .p2align 4
  .type cuGraphExecUpdate, %function
cuGraphExecUpdate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1096(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1096(%rip)
2:
  pushq $137
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphGetEdges
  .p2align 4
  .type cuGraphGetEdges, %function
cuGraphGetEdges:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1104(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1104(%rip)
2:
  pushq $138
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphGetNodes
  .p2align 4
  .type cuGraphGetNodes, %function
cuGraphGetNodes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1112(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1112(%rip)
2:
  pushq $139
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphGetRootNodes
  .p2align 4
  .type cuGraphGetRootNodes, %function
cuGraphGetRootNodes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1120(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1120(%rip)
2:
  pushq $140
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphHostNodeGetParams
  .p2align 4
  .type cuGraphHostNodeGetParams, %function
cuGraphHostNodeGetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1128(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1128(%rip)
2:
  pushq $141
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphHostNodeSetParams
  .p2align 4
  .type cuGraphHostNodeSetParams, %function
cuGraphHostNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1136(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1136(%rip)
2:
  pushq $142
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphInstantiate
  .p2align 4
  .type cuGraphInstantiate, %function
cuGraphInstantiate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1144(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1144(%rip)
2:
  pushq $143
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphInstantiate_v2
  .p2align 4
  .type cuGraphInstantiate_v2, %function
cuGraphInstantiate_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1152(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1152(%rip)
2:
  pushq $144
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphKernelNodeCopyAttributes
  .p2align 4
  .type cuGraphKernelNodeCopyAttributes, %function
cuGraphKernelNodeCopyAttributes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1160(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1160(%rip)
2:
  pushq $145
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphKernelNodeGetAttribute
  .p2align 4
  .type cuGraphKernelNodeGetAttribute, %function
cuGraphKernelNodeGetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1168(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1168(%rip)
2:
  pushq $146
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphKernelNodeGetParams
  .p2align 4
  .type cuGraphKernelNodeGetParams, %function
cuGraphKernelNodeGetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1176(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1176(%rip)
2:
  pushq $147
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphKernelNodeSetAttribute
  .p2align 4
  .type cuGraphKernelNodeSetAttribute, %function
cuGraphKernelNodeSetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1184(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1184(%rip)
2:
  pushq $148
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphKernelNodeSetParams
  .p2align 4
  .type cuGraphKernelNodeSetParams, %function
cuGraphKernelNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1192(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1192(%rip)
2:
  pushq $149
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphLaunch
  .p2align 4
  .type cuGraphLaunch, %function
cuGraphLaunch:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1200(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1200(%rip)
2:
  pushq $150
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphLaunch_ptsz
  .p2align 4
  .type cuGraphLaunch_ptsz, %function
cuGraphLaunch_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1208(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1208(%rip)
2:
  pushq $151
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphMemcpyNodeGetParams
  .p2align 4
  .type cuGraphMemcpyNodeGetParams, %function
cuGraphMemcpyNodeGetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1216(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1216(%rip)
2:
  pushq $152
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphMemcpyNodeSetParams
  .p2align 4
  .type cuGraphMemcpyNodeSetParams, %function
cuGraphMemcpyNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1224(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1224(%rip)
2:
  pushq $153
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphMemsetNodeGetParams
  .p2align 4
  .type cuGraphMemsetNodeGetParams, %function
cuGraphMemsetNodeGetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1232(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1232(%rip)
2:
  pushq $154
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphMemsetNodeSetParams
  .p2align 4
  .type cuGraphMemsetNodeSetParams, %function
cuGraphMemsetNodeSetParams:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1240(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1240(%rip)
2:
  pushq $155
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphNodeFindInClone
  .p2align 4
  .type cuGraphNodeFindInClone, %function
cuGraphNodeFindInClone:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1248(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1248(%rip)
2:
  pushq $156
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphNodeGetDependencies
  .p2align 4
  .type cuGraphNodeGetDependencies, %function
cuGraphNodeGetDependencies:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1256(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1256(%rip)
2:
  pushq $157
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphNodeGetDependentNodes
  .p2align 4
  .type cuGraphNodeGetDependentNodes, %function
cuGraphNodeGetDependentNodes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1264(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1264(%rip)
2:
  pushq $158
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphNodeGetType
  .p2align 4
  .type cuGraphNodeGetType, %function
cuGraphNodeGetType:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1272(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1272(%rip)
2:
  pushq $159
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphRemoveDependencies
  .p2align 4
  .type cuGraphRemoveDependencies, %function
cuGraphRemoveDependencies:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1280(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1280(%rip)
2:
  pushq $160
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphUpload
  .p2align 4
  .type cuGraphUpload, %function
cuGraphUpload:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1288(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1288(%rip)
2:
  pushq $161
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphUpload_ptsz
  .p2align 4
  .type cuGraphUpload_ptsz, %function
cuGraphUpload_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1296(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1296(%rip)
2:
  pushq $162
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsEGLRegisterImage
  .p2align 4
  .type cuGraphicsEGLRegisterImage, %function
cuGraphicsEGLRegisterImage:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1304(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1304(%rip)
2:
  pushq $163
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsGLRegisterBuffer
  .p2align 4
  .type cuGraphicsGLRegisterBuffer, %function
cuGraphicsGLRegisterBuffer:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1312(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1312(%rip)
2:
  pushq $164
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsGLRegisterImage
  .p2align 4
  .type cuGraphicsGLRegisterImage, %function
cuGraphicsGLRegisterImage:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1320(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1320(%rip)
2:
  pushq $165
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsMapResources
  .p2align 4
  .type cuGraphicsMapResources, %function
cuGraphicsMapResources:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1328(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1328(%rip)
2:
  pushq $166
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsMapResources_ptsz
  .p2align 4
  .type cuGraphicsMapResources_ptsz, %function
cuGraphicsMapResources_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1336(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1336(%rip)
2:
  pushq $167
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsResourceGetMappedEglFrame
  .p2align 4
  .type cuGraphicsResourceGetMappedEglFrame, %function
cuGraphicsResourceGetMappedEglFrame:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1344(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1344(%rip)
2:
  pushq $168
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsResourceGetMappedMipmappedArray
  .p2align 4
  .type cuGraphicsResourceGetMappedMipmappedArray, %function
cuGraphicsResourceGetMappedMipmappedArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1352(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1352(%rip)
2:
  pushq $169
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsResourceGetMappedPointer
  .p2align 4
  .type cuGraphicsResourceGetMappedPointer, %function
cuGraphicsResourceGetMappedPointer:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1360(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1360(%rip)
2:
  pushq $170
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsResourceGetMappedPointer_v2
  .p2align 4
  .type cuGraphicsResourceGetMappedPointer_v2, %function
cuGraphicsResourceGetMappedPointer_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1368(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1368(%rip)
2:
  pushq $171
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsResourceSetMapFlags
  .p2align 4
  .type cuGraphicsResourceSetMapFlags, %function
cuGraphicsResourceSetMapFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1376(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1376(%rip)
2:
  pushq $172
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsResourceSetMapFlags_v2
  .p2align 4
  .type cuGraphicsResourceSetMapFlags_v2, %function
cuGraphicsResourceSetMapFlags_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1384(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1384(%rip)
2:
  pushq $173
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsSubResourceGetMappedArray
  .p2align 4
  .type cuGraphicsSubResourceGetMappedArray, %function
cuGraphicsSubResourceGetMappedArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1392(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1392(%rip)
2:
  pushq $174
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsUnmapResources
  .p2align 4
  .type cuGraphicsUnmapResources, %function
cuGraphicsUnmapResources:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1400(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1400(%rip)
2:
  pushq $175
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsUnmapResources_ptsz
  .p2align 4
  .type cuGraphicsUnmapResources_ptsz, %function
cuGraphicsUnmapResources_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1408(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1408(%rip)
2:
  pushq $176
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsUnregisterResource
  .p2align 4
  .type cuGraphicsUnregisterResource, %function
cuGraphicsUnregisterResource:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1416(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1416(%rip)
2:
  pushq $177
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsVDPAURegisterOutputSurface
  .p2align 4
  .type cuGraphicsVDPAURegisterOutputSurface, %function
cuGraphicsVDPAURegisterOutputSurface:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1424(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1424(%rip)
2:
  pushq $178
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuGraphicsVDPAURegisterVideoSurface
  .p2align 4
  .type cuGraphicsVDPAURegisterVideoSurface, %function
cuGraphicsVDPAURegisterVideoSurface:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1432(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1432(%rip)
2:
  pushq $179
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuImportExternalMemory
  .p2align 4
  .type cuImportExternalMemory, %function
cuImportExternalMemory:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1440(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1440(%rip)
2:
  pushq $180
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuImportExternalSemaphore
  .p2align 4
  .type cuImportExternalSemaphore, %function
cuImportExternalSemaphore:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1448(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1448(%rip)
2:
  pushq $181
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuInit
  .p2align 4
  .type cuInit, %function
cuInit:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1456(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1456(%rip)
2:
  pushq $182
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuIpcCloseMemHandle
  .p2align 4
  .type cuIpcCloseMemHandle, %function
cuIpcCloseMemHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1464(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1464(%rip)
2:
  pushq $183
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuIpcGetEventHandle
  .p2align 4
  .type cuIpcGetEventHandle, %function
cuIpcGetEventHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1472(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1472(%rip)
2:
  pushq $184
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuIpcGetMemHandle
  .p2align 4
  .type cuIpcGetMemHandle, %function
cuIpcGetMemHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1480(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1480(%rip)
2:
  pushq $185
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuIpcOpenEventHandle
  .p2align 4
  .type cuIpcOpenEventHandle, %function
cuIpcOpenEventHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1488(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1488(%rip)
2:
  pushq $186
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuIpcOpenMemHandle
  .p2align 4
  .type cuIpcOpenMemHandle, %function
cuIpcOpenMemHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1496(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1496(%rip)
2:
  pushq $187
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuIpcOpenMemHandle_v2
  .p2align 4
  .type cuIpcOpenMemHandle_v2, %function
cuIpcOpenMemHandle_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1504(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1504(%rip)
2:
  pushq $188
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunch
  .p2align 4
  .type cuLaunch, %function
cuLaunch:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1512(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1512(%rip)
2:
  pushq $189
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchCooperativeKernel
  .p2align 4
  .type cuLaunchCooperativeKernel, %function
cuLaunchCooperativeKernel:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1520(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1520(%rip)
2:
  pushq $190
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchCooperativeKernelMultiDevice
  .p2align 4
  .type cuLaunchCooperativeKernelMultiDevice, %function
cuLaunchCooperativeKernelMultiDevice:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1528(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1528(%rip)
2:
  pushq $191
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchCooperativeKernel_ptsz
  .p2align 4
  .type cuLaunchCooperativeKernel_ptsz, %function
cuLaunchCooperativeKernel_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1536(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1536(%rip)
2:
  pushq $192
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchGrid
  .p2align 4
  .type cuLaunchGrid, %function
cuLaunchGrid:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1544(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1544(%rip)
2:
  pushq $193
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchGridAsync
  .p2align 4
  .type cuLaunchGridAsync, %function
cuLaunchGridAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1552(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1552(%rip)
2:
  pushq $194
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchHostFunc
  .p2align 4
  .type cuLaunchHostFunc, %function
cuLaunchHostFunc:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1560(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1560(%rip)
2:
  pushq $195
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchHostFunc_ptsz
  .p2align 4
  .type cuLaunchHostFunc_ptsz, %function
cuLaunchHostFunc_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1568(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1568(%rip)
2:
  pushq $196
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchKernel
  .p2align 4
  .type cuLaunchKernel, %function
cuLaunchKernel:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1576(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1576(%rip)
2:
  pushq $197
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLaunchKernel_ptsz
  .p2align 4
  .type cuLaunchKernel_ptsz, %function
cuLaunchKernel_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1584(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1584(%rip)
2:
  pushq $198
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkAddData
  .p2align 4
  .type cuLinkAddData, %function
cuLinkAddData:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1592(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1592(%rip)
2:
  pushq $199
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkAddData_v2
  .p2align 4
  .type cuLinkAddData_v2, %function
cuLinkAddData_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1600(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1600(%rip)
2:
  pushq $200
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkAddFile
  .p2align 4
  .type cuLinkAddFile, %function
cuLinkAddFile:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1608(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1608(%rip)
2:
  pushq $201
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkAddFile_v2
  .p2align 4
  .type cuLinkAddFile_v2, %function
cuLinkAddFile_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1616(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1616(%rip)
2:
  pushq $202
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkComplete
  .p2align 4
  .type cuLinkComplete, %function
cuLinkComplete:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1624(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1624(%rip)
2:
  pushq $203
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkCreate
  .p2align 4
  .type cuLinkCreate, %function
cuLinkCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1632(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1632(%rip)
2:
  pushq $204
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkCreate_v2
  .p2align 4
  .type cuLinkCreate_v2, %function
cuLinkCreate_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1640(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1640(%rip)
2:
  pushq $205
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuLinkDestroy
  .p2align 4
  .type cuLinkDestroy, %function
cuLinkDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1648(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1648(%rip)
2:
  pushq $206
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAddressFree
  .p2align 4
  .type cuMemAddressFree, %function
cuMemAddressFree:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1656(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1656(%rip)
2:
  pushq $207
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAddressReserve
  .p2align 4
  .type cuMemAddressReserve, %function
cuMemAddressReserve:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1664(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1664(%rip)
2:
  pushq $208
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAdvise
  .p2align 4
  .type cuMemAdvise, %function
cuMemAdvise:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1672(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1672(%rip)
2:
  pushq $209
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAlloc
  .p2align 4
  .type cuMemAlloc, %function
cuMemAlloc:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1680(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1680(%rip)
2:
  pushq $210
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAllocHost
  .p2align 4
  .type cuMemAllocHost, %function
cuMemAllocHost:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1688(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1688(%rip)
2:
  pushq $211
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAllocHost_v2
  .p2align 4
  .type cuMemAllocHost_v2, %function
cuMemAllocHost_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1696(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1696(%rip)
2:
  pushq $212
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAllocManaged
  .p2align 4
  .type cuMemAllocManaged, %function
cuMemAllocManaged:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1704(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1704(%rip)
2:
  pushq $213
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAllocPitch
  .p2align 4
  .type cuMemAllocPitch, %function
cuMemAllocPitch:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1712(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1712(%rip)
2:
  pushq $214
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAllocPitch_v2
  .p2align 4
  .type cuMemAllocPitch_v2, %function
cuMemAllocPitch_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1720(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1720(%rip)
2:
  pushq $215
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemAlloc_v2
  .p2align 4
  .type cuMemAlloc_v2, %function
cuMemAlloc_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1728(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1728(%rip)
2:
  pushq $216
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemCreate
  .p2align 4
  .type cuMemCreate, %function
cuMemCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1736(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1736(%rip)
2:
  pushq $217
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemExportToShareableHandle
  .p2align 4
  .type cuMemExportToShareableHandle, %function
cuMemExportToShareableHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1744(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1744(%rip)
2:
  pushq $218
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemFree
  .p2align 4
  .type cuMemFree, %function
cuMemFree:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1752(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1752(%rip)
2:
  pushq $219
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemFreeHost
  .p2align 4
  .type cuMemFreeHost, %function
cuMemFreeHost:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1760(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1760(%rip)
2:
  pushq $220
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemFree_v2
  .p2align 4
  .type cuMemFree_v2, %function
cuMemFree_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1768(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1768(%rip)
2:
  pushq $221
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetAccess
  .p2align 4
  .type cuMemGetAccess, %function
cuMemGetAccess:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1776(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1776(%rip)
2:
  pushq $222
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetAddressRange
  .p2align 4
  .type cuMemGetAddressRange, %function
cuMemGetAddressRange:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1784(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1784(%rip)
2:
  pushq $223
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetAddressRange_v2
  .p2align 4
  .type cuMemGetAddressRange_v2, %function
cuMemGetAddressRange_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1792(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1792(%rip)
2:
  pushq $224
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetAllocationGranularity
  .p2align 4
  .type cuMemGetAllocationGranularity, %function
cuMemGetAllocationGranularity:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1800(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1800(%rip)
2:
  pushq $225
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetAllocationPropertiesFromHandle
  .p2align 4
  .type cuMemGetAllocationPropertiesFromHandle, %function
cuMemGetAllocationPropertiesFromHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1808(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1808(%rip)
2:
  pushq $226
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetAttribute
  .p2align 4
  .type cuMemGetAttribute, %function
cuMemGetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1816(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1816(%rip)
2:
  pushq $227
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetAttribute_v2
  .p2align 4
  .type cuMemGetAttribute_v2, %function
cuMemGetAttribute_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1824(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1824(%rip)
2:
  pushq $228
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetInfo
  .p2align 4
  .type cuMemGetInfo, %function
cuMemGetInfo:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1832(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1832(%rip)
2:
  pushq $229
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemGetInfo_v2
  .p2align 4
  .type cuMemGetInfo_v2, %function
cuMemGetInfo_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1840(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1840(%rip)
2:
  pushq $230
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemHostAlloc
  .p2align 4
  .type cuMemHostAlloc, %function
cuMemHostAlloc:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1848(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1848(%rip)
2:
  pushq $231
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemHostGetDevicePointer
  .p2align 4
  .type cuMemHostGetDevicePointer, %function
cuMemHostGetDevicePointer:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1856(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1856(%rip)
2:
  pushq $232
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemHostGetDevicePointer_v2
  .p2align 4
  .type cuMemHostGetDevicePointer_v2, %function
cuMemHostGetDevicePointer_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1864(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1864(%rip)
2:
  pushq $233
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemHostGetFlags
  .p2align 4
  .type cuMemHostGetFlags, %function
cuMemHostGetFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1872(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1872(%rip)
2:
  pushq $234
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemHostRegister
  .p2align 4
  .type cuMemHostRegister, %function
cuMemHostRegister:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1880(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1880(%rip)
2:
  pushq $235
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemHostRegister_v2
  .p2align 4
  .type cuMemHostRegister_v2, %function
cuMemHostRegister_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1888(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1888(%rip)
2:
  pushq $236
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemHostUnregister
  .p2align 4
  .type cuMemHostUnregister, %function
cuMemHostUnregister:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1896(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1896(%rip)
2:
  pushq $237
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemImportFromShareableHandle
  .p2align 4
  .type cuMemImportFromShareableHandle, %function
cuMemImportFromShareableHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1904(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1904(%rip)
2:
  pushq $238
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemMap
  .p2align 4
  .type cuMemMap, %function
cuMemMap:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1912(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1912(%rip)
2:
  pushq $239
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemMapArrayAsync
  .p2align 4
  .type cuMemMapArrayAsync, %function
cuMemMapArrayAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1920(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1920(%rip)
2:
  pushq $240
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemMapArrayAsync_ptsz
  .p2align 4
  .type cuMemMapArrayAsync_ptsz, %function
cuMemMapArrayAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1928(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1928(%rip)
2:
  pushq $241
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemPrefetchAsync
  .p2align 4
  .type cuMemPrefetchAsync, %function
cuMemPrefetchAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1936(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1936(%rip)
2:
  pushq $242
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemPrefetchAsync_ptsz
  .p2align 4
  .type cuMemPrefetchAsync_ptsz, %function
cuMemPrefetchAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1944(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1944(%rip)
2:
  pushq $243
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemRangeGetAttribute
  .p2align 4
  .type cuMemRangeGetAttribute, %function
cuMemRangeGetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1952(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1952(%rip)
2:
  pushq $244
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemRangeGetAttributes
  .p2align 4
  .type cuMemRangeGetAttributes, %function
cuMemRangeGetAttributes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1960(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1960(%rip)
2:
  pushq $245
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemRelease
  .p2align 4
  .type cuMemRelease, %function
cuMemRelease:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1968(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1968(%rip)
2:
  pushq $246
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemRetainAllocationHandle
  .p2align 4
  .type cuMemRetainAllocationHandle, %function
cuMemRetainAllocationHandle:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1976(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1976(%rip)
2:
  pushq $247
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemSetAccess
  .p2align 4
  .type cuMemSetAccess, %function
cuMemSetAccess:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1984(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1984(%rip)
2:
  pushq $248
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemUnmap
  .p2align 4
  .type cuMemUnmap, %function
cuMemUnmap:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+1992(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+1992(%rip)
2:
  pushq $249
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy
  .p2align 4
  .type cuMemcpy, %function
cuMemcpy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2000(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2000(%rip)
2:
  pushq $250
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2D
  .p2align 4
  .type cuMemcpy2D, %function
cuMemcpy2D:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2008(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2008(%rip)
2:
  pushq $251
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2DAsync
  .p2align 4
  .type cuMemcpy2DAsync, %function
cuMemcpy2DAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2016(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2016(%rip)
2:
  pushq $252
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2DAsync_v2
  .p2align 4
  .type cuMemcpy2DAsync_v2, %function
cuMemcpy2DAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2024(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2024(%rip)
2:
  pushq $253
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2DAsync_v2_ptsz
  .p2align 4
  .type cuMemcpy2DAsync_v2_ptsz, %function
cuMemcpy2DAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2032(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2032(%rip)
2:
  pushq $254
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2DUnaligned
  .p2align 4
  .type cuMemcpy2DUnaligned, %function
cuMemcpy2DUnaligned:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2040(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2040(%rip)
2:
  pushq $255
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2DUnaligned_v2
  .p2align 4
  .type cuMemcpy2DUnaligned_v2, %function
cuMemcpy2DUnaligned_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2048(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2048(%rip)
2:
  pushq $256
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2DUnaligned_v2_ptds
  .p2align 4
  .type cuMemcpy2DUnaligned_v2_ptds, %function
cuMemcpy2DUnaligned_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2056(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2056(%rip)
2:
  pushq $257
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2D_v2
  .p2align 4
  .type cuMemcpy2D_v2, %function
cuMemcpy2D_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2064(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2064(%rip)
2:
  pushq $258
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy2D_v2_ptds
  .p2align 4
  .type cuMemcpy2D_v2_ptds, %function
cuMemcpy2D_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2072(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2072(%rip)
2:
  pushq $259
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3D
  .p2align 4
  .type cuMemcpy3D, %function
cuMemcpy3D:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2080(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2080(%rip)
2:
  pushq $260
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3DAsync
  .p2align 4
  .type cuMemcpy3DAsync, %function
cuMemcpy3DAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2088(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2088(%rip)
2:
  pushq $261
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3DAsync_v2
  .p2align 4
  .type cuMemcpy3DAsync_v2, %function
cuMemcpy3DAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2096(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2096(%rip)
2:
  pushq $262
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3DAsync_v2_ptsz
  .p2align 4
  .type cuMemcpy3DAsync_v2_ptsz, %function
cuMemcpy3DAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2104(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2104(%rip)
2:
  pushq $263
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3DPeer
  .p2align 4
  .type cuMemcpy3DPeer, %function
cuMemcpy3DPeer:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2112(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2112(%rip)
2:
  pushq $264
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3DPeerAsync
  .p2align 4
  .type cuMemcpy3DPeerAsync, %function
cuMemcpy3DPeerAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2120(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2120(%rip)
2:
  pushq $265
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3DPeerAsync_ptsz
  .p2align 4
  .type cuMemcpy3DPeerAsync_ptsz, %function
cuMemcpy3DPeerAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2128(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2128(%rip)
2:
  pushq $266
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3DPeer_ptds
  .p2align 4
  .type cuMemcpy3DPeer_ptds, %function
cuMemcpy3DPeer_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2136(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2136(%rip)
2:
  pushq $267
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3D_v2
  .p2align 4
  .type cuMemcpy3D_v2, %function
cuMemcpy3D_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2144(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2144(%rip)
2:
  pushq $268
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy3D_v2_ptds
  .p2align 4
  .type cuMemcpy3D_v2_ptds, %function
cuMemcpy3D_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2152(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2152(%rip)
2:
  pushq $269
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAsync
  .p2align 4
  .type cuMemcpyAsync, %function
cuMemcpyAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2160(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2160(%rip)
2:
  pushq $270
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAsync_ptsz
  .p2align 4
  .type cuMemcpyAsync_ptsz, %function
cuMemcpyAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2168(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2168(%rip)
2:
  pushq $271
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoA
  .p2align 4
  .type cuMemcpyAtoA, %function
cuMemcpyAtoA:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2176(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2176(%rip)
2:
  pushq $272
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoA_v2
  .p2align 4
  .type cuMemcpyAtoA_v2, %function
cuMemcpyAtoA_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2184(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2184(%rip)
2:
  pushq $273
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoA_v2_ptds
  .p2align 4
  .type cuMemcpyAtoA_v2_ptds, %function
cuMemcpyAtoA_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2192(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2192(%rip)
2:
  pushq $274
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoD
  .p2align 4
  .type cuMemcpyAtoD, %function
cuMemcpyAtoD:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2200(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2200(%rip)
2:
  pushq $275
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoD_v2
  .p2align 4
  .type cuMemcpyAtoD_v2, %function
cuMemcpyAtoD_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2208(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2208(%rip)
2:
  pushq $276
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoD_v2_ptds
  .p2align 4
  .type cuMemcpyAtoD_v2_ptds, %function
cuMemcpyAtoD_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2216(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2216(%rip)
2:
  pushq $277
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoH
  .p2align 4
  .type cuMemcpyAtoH, %function
cuMemcpyAtoH:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2224(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2224(%rip)
2:
  pushq $278
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoHAsync
  .p2align 4
  .type cuMemcpyAtoHAsync, %function
cuMemcpyAtoHAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2232(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2232(%rip)
2:
  pushq $279
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoHAsync_v2
  .p2align 4
  .type cuMemcpyAtoHAsync_v2, %function
cuMemcpyAtoHAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2240(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2240(%rip)
2:
  pushq $280
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoHAsync_v2_ptsz
  .p2align 4
  .type cuMemcpyAtoHAsync_v2_ptsz, %function
cuMemcpyAtoHAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2248(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2248(%rip)
2:
  pushq $281
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoH_v2
  .p2align 4
  .type cuMemcpyAtoH_v2, %function
cuMemcpyAtoH_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2256(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2256(%rip)
2:
  pushq $282
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyAtoH_v2_ptds
  .p2align 4
  .type cuMemcpyAtoH_v2_ptds, %function
cuMemcpyAtoH_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2264(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2264(%rip)
2:
  pushq $283
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoA
  .p2align 4
  .type cuMemcpyDtoA, %function
cuMemcpyDtoA:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2272(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2272(%rip)
2:
  pushq $284
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoA_v2
  .p2align 4
  .type cuMemcpyDtoA_v2, %function
cuMemcpyDtoA_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2280(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2280(%rip)
2:
  pushq $285
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoA_v2_ptds
  .p2align 4
  .type cuMemcpyDtoA_v2_ptds, %function
cuMemcpyDtoA_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2288(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2288(%rip)
2:
  pushq $286
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoD
  .p2align 4
  .type cuMemcpyDtoD, %function
cuMemcpyDtoD:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2296(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2296(%rip)
2:
  pushq $287
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoDAsync
  .p2align 4
  .type cuMemcpyDtoDAsync, %function
cuMemcpyDtoDAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2304(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2304(%rip)
2:
  pushq $288
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoDAsync_v2
  .p2align 4
  .type cuMemcpyDtoDAsync_v2, %function
cuMemcpyDtoDAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2312(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2312(%rip)
2:
  pushq $289
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoDAsync_v2_ptsz
  .p2align 4
  .type cuMemcpyDtoDAsync_v2_ptsz, %function
cuMemcpyDtoDAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2320(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2320(%rip)
2:
  pushq $290
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoD_v2
  .p2align 4
  .type cuMemcpyDtoD_v2, %function
cuMemcpyDtoD_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2328(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2328(%rip)
2:
  pushq $291
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoD_v2_ptds
  .p2align 4
  .type cuMemcpyDtoD_v2_ptds, %function
cuMemcpyDtoD_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2336(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2336(%rip)
2:
  pushq $292
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoH
  .p2align 4
  .type cuMemcpyDtoH, %function
cuMemcpyDtoH:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2344(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2344(%rip)
2:
  pushq $293
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoHAsync
  .p2align 4
  .type cuMemcpyDtoHAsync, %function
cuMemcpyDtoHAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2352(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2352(%rip)
2:
  pushq $294
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoHAsync_v2
  .p2align 4
  .type cuMemcpyDtoHAsync_v2, %function
cuMemcpyDtoHAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2360(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2360(%rip)
2:
  pushq $295
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoHAsync_v2_ptsz
  .p2align 4
  .type cuMemcpyDtoHAsync_v2_ptsz, %function
cuMemcpyDtoHAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2368(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2368(%rip)
2:
  pushq $296
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoH_v2
  .p2align 4
  .type cuMemcpyDtoH_v2, %function
cuMemcpyDtoH_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2376(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2376(%rip)
2:
  pushq $297
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyDtoH_v2_ptds
  .p2align 4
  .type cuMemcpyDtoH_v2_ptds, %function
cuMemcpyDtoH_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2384(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2384(%rip)
2:
  pushq $298
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoA
  .p2align 4
  .type cuMemcpyHtoA, %function
cuMemcpyHtoA:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2392(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2392(%rip)
2:
  pushq $299
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoAAsync
  .p2align 4
  .type cuMemcpyHtoAAsync, %function
cuMemcpyHtoAAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2400(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2400(%rip)
2:
  pushq $300
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoAAsync_v2
  .p2align 4
  .type cuMemcpyHtoAAsync_v2, %function
cuMemcpyHtoAAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2408(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2408(%rip)
2:
  pushq $301
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoAAsync_v2_ptsz
  .p2align 4
  .type cuMemcpyHtoAAsync_v2_ptsz, %function
cuMemcpyHtoAAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2416(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2416(%rip)
2:
  pushq $302
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoA_v2
  .p2align 4
  .type cuMemcpyHtoA_v2, %function
cuMemcpyHtoA_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2424(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2424(%rip)
2:
  pushq $303
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoA_v2_ptds
  .p2align 4
  .type cuMemcpyHtoA_v2_ptds, %function
cuMemcpyHtoA_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2432(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2432(%rip)
2:
  pushq $304
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoD
  .p2align 4
  .type cuMemcpyHtoD, %function
cuMemcpyHtoD:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2440(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2440(%rip)
2:
  pushq $305
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoDAsync
  .p2align 4
  .type cuMemcpyHtoDAsync, %function
cuMemcpyHtoDAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2448(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2448(%rip)
2:
  pushq $306
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoDAsync_v2
  .p2align 4
  .type cuMemcpyHtoDAsync_v2, %function
cuMemcpyHtoDAsync_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2456(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2456(%rip)
2:
  pushq $307
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoDAsync_v2_ptsz
  .p2align 4
  .type cuMemcpyHtoDAsync_v2_ptsz, %function
cuMemcpyHtoDAsync_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2464(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2464(%rip)
2:
  pushq $308
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoD_v2
  .p2align 4
  .type cuMemcpyHtoD_v2, %function
cuMemcpyHtoD_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2472(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2472(%rip)
2:
  pushq $309
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyHtoD_v2_ptds
  .p2align 4
  .type cuMemcpyHtoD_v2_ptds, %function
cuMemcpyHtoD_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2480(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2480(%rip)
2:
  pushq $310
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyPeer
  .p2align 4
  .type cuMemcpyPeer, %function
cuMemcpyPeer:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2488(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2488(%rip)
2:
  pushq $311
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyPeerAsync
  .p2align 4
  .type cuMemcpyPeerAsync, %function
cuMemcpyPeerAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2496(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2496(%rip)
2:
  pushq $312
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyPeerAsync_ptsz
  .p2align 4
  .type cuMemcpyPeerAsync_ptsz, %function
cuMemcpyPeerAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2504(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2504(%rip)
2:
  pushq $313
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpyPeer_ptds
  .p2align 4
  .type cuMemcpyPeer_ptds, %function
cuMemcpyPeer_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2512(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2512(%rip)
2:
  pushq $314
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemcpy_ptds
  .p2align 4
  .type cuMemcpy_ptds, %function
cuMemcpy_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2520(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2520(%rip)
2:
  pushq $315
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD16
  .p2align 4
  .type cuMemsetD16, %function
cuMemsetD16:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2528(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2528(%rip)
2:
  pushq $316
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD16Async
  .p2align 4
  .type cuMemsetD16Async, %function
cuMemsetD16Async:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2536(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2536(%rip)
2:
  pushq $317
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD16Async_ptsz
  .p2align 4
  .type cuMemsetD16Async_ptsz, %function
cuMemsetD16Async_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2544(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2544(%rip)
2:
  pushq $318
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD16_v2
  .p2align 4
  .type cuMemsetD16_v2, %function
cuMemsetD16_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2552(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2552(%rip)
2:
  pushq $319
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD16_v2_ptds
  .p2align 4
  .type cuMemsetD16_v2_ptds, %function
cuMemsetD16_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2560(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2560(%rip)
2:
  pushq $320
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D16
  .p2align 4
  .type cuMemsetD2D16, %function
cuMemsetD2D16:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2568(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2568(%rip)
2:
  pushq $321
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D16Async
  .p2align 4
  .type cuMemsetD2D16Async, %function
cuMemsetD2D16Async:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2576(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2576(%rip)
2:
  pushq $322
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D16Async_ptsz
  .p2align 4
  .type cuMemsetD2D16Async_ptsz, %function
cuMemsetD2D16Async_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2584(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2584(%rip)
2:
  pushq $323
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D16_v2
  .p2align 4
  .type cuMemsetD2D16_v2, %function
cuMemsetD2D16_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2592(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2592(%rip)
2:
  pushq $324
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D16_v2_ptds
  .p2align 4
  .type cuMemsetD2D16_v2_ptds, %function
cuMemsetD2D16_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2600(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2600(%rip)
2:
  pushq $325
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D32
  .p2align 4
  .type cuMemsetD2D32, %function
cuMemsetD2D32:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2608(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2608(%rip)
2:
  pushq $326
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D32Async
  .p2align 4
  .type cuMemsetD2D32Async, %function
cuMemsetD2D32Async:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2616(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2616(%rip)
2:
  pushq $327
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D32Async_ptsz
  .p2align 4
  .type cuMemsetD2D32Async_ptsz, %function
cuMemsetD2D32Async_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2624(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2624(%rip)
2:
  pushq $328
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D32_v2
  .p2align 4
  .type cuMemsetD2D32_v2, %function
cuMemsetD2D32_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2632(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2632(%rip)
2:
  pushq $329
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D32_v2_ptds
  .p2align 4
  .type cuMemsetD2D32_v2_ptds, %function
cuMemsetD2D32_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2640(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2640(%rip)
2:
  pushq $330
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D8
  .p2align 4
  .type cuMemsetD2D8, %function
cuMemsetD2D8:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2648(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2648(%rip)
2:
  pushq $331
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D8Async
  .p2align 4
  .type cuMemsetD2D8Async, %function
cuMemsetD2D8Async:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2656(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2656(%rip)
2:
  pushq $332
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D8Async_ptsz
  .p2align 4
  .type cuMemsetD2D8Async_ptsz, %function
cuMemsetD2D8Async_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2664(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2664(%rip)
2:
  pushq $333
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D8_v2
  .p2align 4
  .type cuMemsetD2D8_v2, %function
cuMemsetD2D8_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2672(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2672(%rip)
2:
  pushq $334
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD2D8_v2_ptds
  .p2align 4
  .type cuMemsetD2D8_v2_ptds, %function
cuMemsetD2D8_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2680(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2680(%rip)
2:
  pushq $335
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD32
  .p2align 4
  .type cuMemsetD32, %function
cuMemsetD32:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2688(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2688(%rip)
2:
  pushq $336
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD32Async
  .p2align 4
  .type cuMemsetD32Async, %function
cuMemsetD32Async:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2696(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2696(%rip)
2:
  pushq $337
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD32Async_ptsz
  .p2align 4
  .type cuMemsetD32Async_ptsz, %function
cuMemsetD32Async_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2704(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2704(%rip)
2:
  pushq $338
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD32_v2
  .p2align 4
  .type cuMemsetD32_v2, %function
cuMemsetD32_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2712(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2712(%rip)
2:
  pushq $339
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD32_v2_ptds
  .p2align 4
  .type cuMemsetD32_v2_ptds, %function
cuMemsetD32_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2720(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2720(%rip)
2:
  pushq $340
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD8
  .p2align 4
  .type cuMemsetD8, %function
cuMemsetD8:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2728(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2728(%rip)
2:
  pushq $341
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD8Async
  .p2align 4
  .type cuMemsetD8Async, %function
cuMemsetD8Async:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2736(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2736(%rip)
2:
  pushq $342
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD8Async_ptsz
  .p2align 4
  .type cuMemsetD8Async_ptsz, %function
cuMemsetD8Async_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2744(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2744(%rip)
2:
  pushq $343
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD8_v2
  .p2align 4
  .type cuMemsetD8_v2, %function
cuMemsetD8_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2752(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2752(%rip)
2:
  pushq $344
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMemsetD8_v2_ptds
  .p2align 4
  .type cuMemsetD8_v2_ptds, %function
cuMemsetD8_v2_ptds:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2760(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2760(%rip)
2:
  pushq $345
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMipmappedArrayCreate
  .p2align 4
  .type cuMipmappedArrayCreate, %function
cuMipmappedArrayCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2768(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2768(%rip)
2:
  pushq $346
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMipmappedArrayDestroy
  .p2align 4
  .type cuMipmappedArrayDestroy, %function
cuMipmappedArrayDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2776(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2776(%rip)
2:
  pushq $347
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMipmappedArrayGetLevel
  .p2align 4
  .type cuMipmappedArrayGetLevel, %function
cuMipmappedArrayGetLevel:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2784(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2784(%rip)
2:
  pushq $348
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuMipmappedArrayGetSparseProperties
  .p2align 4
  .type cuMipmappedArrayGetSparseProperties, %function
cuMipmappedArrayGetSparseProperties:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2792(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2792(%rip)
2:
  pushq $349
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleGetFunction
  .p2align 4
  .type cuModuleGetFunction, %function
cuModuleGetFunction:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2800(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2800(%rip)
2:
  pushq $350
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleGetGlobal
  .p2align 4
  .type cuModuleGetGlobal, %function
cuModuleGetGlobal:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2808(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2808(%rip)
2:
  pushq $351
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleGetGlobal_v2
  .p2align 4
  .type cuModuleGetGlobal_v2, %function
cuModuleGetGlobal_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2816(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2816(%rip)
2:
  pushq $352
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleGetSurfRef
  .p2align 4
  .type cuModuleGetSurfRef, %function
cuModuleGetSurfRef:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2824(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2824(%rip)
2:
  pushq $353
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleGetTexRef
  .p2align 4
  .type cuModuleGetTexRef, %function
cuModuleGetTexRef:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2832(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2832(%rip)
2:
  pushq $354
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleLoad
  .p2align 4
  .type cuModuleLoad, %function
cuModuleLoad:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2840(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2840(%rip)
2:
  pushq $355
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleLoadData
  .p2align 4
  .type cuModuleLoadData, %function
cuModuleLoadData:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2848(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2848(%rip)
2:
  pushq $356
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleLoadDataEx
  .p2align 4
  .type cuModuleLoadDataEx, %function
cuModuleLoadDataEx:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2856(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2856(%rip)
2:
  pushq $357
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleLoadFatBinary
  .p2align 4
  .type cuModuleLoadFatBinary, %function
cuModuleLoadFatBinary:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2864(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2864(%rip)
2:
  pushq $358
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuModuleUnload
  .p2align 4
  .type cuModuleUnload, %function
cuModuleUnload:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2872(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2872(%rip)
2:
  pushq $359
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuOccupancyAvailableDynamicSMemPerBlock
  .p2align 4
  .type cuOccupancyAvailableDynamicSMemPerBlock, %function
cuOccupancyAvailableDynamicSMemPerBlock:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2880(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2880(%rip)
2:
  pushq $360
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuOccupancyMaxActiveBlocksPerMultiprocessor
  .p2align 4
  .type cuOccupancyMaxActiveBlocksPerMultiprocessor, %function
cuOccupancyMaxActiveBlocksPerMultiprocessor:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2888(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2888(%rip)
2:
  pushq $361
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuOccupancyMaxActiveBlocksPerMultiprocessorWithFlags
  .p2align 4
  .type cuOccupancyMaxActiveBlocksPerMultiprocessorWithFlags, %function
cuOccupancyMaxActiveBlocksPerMultiprocessorWithFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2896(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2896(%rip)
2:
  pushq $362
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuOccupancyMaxPotentialBlockSize
  .p2align 4
  .type cuOccupancyMaxPotentialBlockSize, %function
cuOccupancyMaxPotentialBlockSize:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2904(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2904(%rip)
2:
  pushq $363
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuOccupancyMaxPotentialBlockSizeWithFlags
  .p2align 4
  .type cuOccupancyMaxPotentialBlockSizeWithFlags, %function
cuOccupancyMaxPotentialBlockSizeWithFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2912(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2912(%rip)
2:
  pushq $364
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuParamSetSize
  .p2align 4
  .type cuParamSetSize, %function
cuParamSetSize:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2920(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2920(%rip)
2:
  pushq $365
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuParamSetTexRef
  .p2align 4
  .type cuParamSetTexRef, %function
cuParamSetTexRef:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2928(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2928(%rip)
2:
  pushq $366
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuParamSetf
  .p2align 4
  .type cuParamSetf, %function
cuParamSetf:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2936(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2936(%rip)
2:
  pushq $367
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuParamSeti
  .p2align 4
  .type cuParamSeti, %function
cuParamSeti:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2944(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2944(%rip)
2:
  pushq $368
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuParamSetv
  .p2align 4
  .type cuParamSetv, %function
cuParamSetv:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2952(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2952(%rip)
2:
  pushq $369
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuPointerGetAttribute
  .p2align 4
  .type cuPointerGetAttribute, %function
cuPointerGetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2960(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2960(%rip)
2:
  pushq $370
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuPointerGetAttributes
  .p2align 4
  .type cuPointerGetAttributes, %function
cuPointerGetAttributes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2968(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2968(%rip)
2:
  pushq $371
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuPointerSetAttribute
  .p2align 4
  .type cuPointerSetAttribute, %function
cuPointerSetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2976(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2976(%rip)
2:
  pushq $372
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuProfilerInitialize
  .p2align 4
  .type cuProfilerInitialize, %function
cuProfilerInitialize:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2984(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2984(%rip)
2:
  pushq $373
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuProfilerStart
  .p2align 4
  .type cuProfilerStart, %function
cuProfilerStart:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+2992(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+2992(%rip)
2:
  pushq $374
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuProfilerStop
  .p2align 4
  .type cuProfilerStop, %function
cuProfilerStop:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3000(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3000(%rip)
2:
  pushq $375
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuSignalExternalSemaphoresAsync
  .p2align 4
  .type cuSignalExternalSemaphoresAsync, %function
cuSignalExternalSemaphoresAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3008(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3008(%rip)
2:
  pushq $376
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuSignalExternalSemaphoresAsync_ptsz
  .p2align 4
  .type cuSignalExternalSemaphoresAsync_ptsz, %function
cuSignalExternalSemaphoresAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3016(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3016(%rip)
2:
  pushq $377
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamAddCallback
  .p2align 4
  .type cuStreamAddCallback, %function
cuStreamAddCallback:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3024(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3024(%rip)
2:
  pushq $378
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamAddCallback_ptsz
  .p2align 4
  .type cuStreamAddCallback_ptsz, %function
cuStreamAddCallback_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3032(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3032(%rip)
2:
  pushq $379
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamAttachMemAsync
  .p2align 4
  .type cuStreamAttachMemAsync, %function
cuStreamAttachMemAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3040(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3040(%rip)
2:
  pushq $380
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamAttachMemAsync_ptsz
  .p2align 4
  .type cuStreamAttachMemAsync_ptsz, %function
cuStreamAttachMemAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3048(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3048(%rip)
2:
  pushq $381
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamBatchMemOp
  .p2align 4
  .type cuStreamBatchMemOp, %function
cuStreamBatchMemOp:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3056(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3056(%rip)
2:
  pushq $382
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamBatchMemOp_ptsz
  .p2align 4
  .type cuStreamBatchMemOp_ptsz, %function
cuStreamBatchMemOp_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3064(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3064(%rip)
2:
  pushq $383
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamBeginCapture
  .p2align 4
  .type cuStreamBeginCapture, %function
cuStreamBeginCapture:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3072(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3072(%rip)
2:
  pushq $384
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamBeginCapture_ptsz
  .p2align 4
  .type cuStreamBeginCapture_ptsz, %function
cuStreamBeginCapture_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3080(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3080(%rip)
2:
  pushq $385
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamBeginCapture_v2
  .p2align 4
  .type cuStreamBeginCapture_v2, %function
cuStreamBeginCapture_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3088(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3088(%rip)
2:
  pushq $386
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamBeginCapture_v2_ptsz
  .p2align 4
  .type cuStreamBeginCapture_v2_ptsz, %function
cuStreamBeginCapture_v2_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3096(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3096(%rip)
2:
  pushq $387
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamCopyAttributes
  .p2align 4
  .type cuStreamCopyAttributes, %function
cuStreamCopyAttributes:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3104(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3104(%rip)
2:
  pushq $388
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamCopyAttributes_ptsz
  .p2align 4
  .type cuStreamCopyAttributes_ptsz, %function
cuStreamCopyAttributes_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3112(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3112(%rip)
2:
  pushq $389
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamCreate
  .p2align 4
  .type cuStreamCreate, %function
cuStreamCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3120(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3120(%rip)
2:
  pushq $390
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamCreateWithPriority
  .p2align 4
  .type cuStreamCreateWithPriority, %function
cuStreamCreateWithPriority:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3128(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3128(%rip)
2:
  pushq $391
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamDestroy
  .p2align 4
  .type cuStreamDestroy, %function
cuStreamDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3136(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3136(%rip)
2:
  pushq $392
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamDestroy_v2
  .p2align 4
  .type cuStreamDestroy_v2, %function
cuStreamDestroy_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3144(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3144(%rip)
2:
  pushq $393
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamEndCapture
  .p2align 4
  .type cuStreamEndCapture, %function
cuStreamEndCapture:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3152(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3152(%rip)
2:
  pushq $394
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamEndCapture_ptsz
  .p2align 4
  .type cuStreamEndCapture_ptsz, %function
cuStreamEndCapture_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3160(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3160(%rip)
2:
  pushq $395
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetAttribute
  .p2align 4
  .type cuStreamGetAttribute, %function
cuStreamGetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3168(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3168(%rip)
2:
  pushq $396
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetAttribute_ptsz
  .p2align 4
  .type cuStreamGetAttribute_ptsz, %function
cuStreamGetAttribute_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3176(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3176(%rip)
2:
  pushq $397
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetCaptureInfo
  .p2align 4
  .type cuStreamGetCaptureInfo, %function
cuStreamGetCaptureInfo:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3184(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3184(%rip)
2:
  pushq $398
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetCaptureInfo_ptsz
  .p2align 4
  .type cuStreamGetCaptureInfo_ptsz, %function
cuStreamGetCaptureInfo_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3192(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3192(%rip)
2:
  pushq $399
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetCtx
  .p2align 4
  .type cuStreamGetCtx, %function
cuStreamGetCtx:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3200(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3200(%rip)
2:
  pushq $400
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetCtx_ptsz
  .p2align 4
  .type cuStreamGetCtx_ptsz, %function
cuStreamGetCtx_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3208(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3208(%rip)
2:
  pushq $401
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetFlags
  .p2align 4
  .type cuStreamGetFlags, %function
cuStreamGetFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3216(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3216(%rip)
2:
  pushq $402
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetFlags_ptsz
  .p2align 4
  .type cuStreamGetFlags_ptsz, %function
cuStreamGetFlags_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3224(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3224(%rip)
2:
  pushq $403
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetPriority
  .p2align 4
  .type cuStreamGetPriority, %function
cuStreamGetPriority:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3232(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3232(%rip)
2:
  pushq $404
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamGetPriority_ptsz
  .p2align 4
  .type cuStreamGetPriority_ptsz, %function
cuStreamGetPriority_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3240(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3240(%rip)
2:
  pushq $405
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamIsCapturing
  .p2align 4
  .type cuStreamIsCapturing, %function
cuStreamIsCapturing:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3248(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3248(%rip)
2:
  pushq $406
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamIsCapturing_ptsz
  .p2align 4
  .type cuStreamIsCapturing_ptsz, %function
cuStreamIsCapturing_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3256(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3256(%rip)
2:
  pushq $407
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamQuery
  .p2align 4
  .type cuStreamQuery, %function
cuStreamQuery:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3264(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3264(%rip)
2:
  pushq $408
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamQuery_ptsz
  .p2align 4
  .type cuStreamQuery_ptsz, %function
cuStreamQuery_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3272(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3272(%rip)
2:
  pushq $409
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamSetAttribute
  .p2align 4
  .type cuStreamSetAttribute, %function
cuStreamSetAttribute:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3280(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3280(%rip)
2:
  pushq $410
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamSetAttribute_ptsz
  .p2align 4
  .type cuStreamSetAttribute_ptsz, %function
cuStreamSetAttribute_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3288(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3288(%rip)
2:
  pushq $411
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamSynchronize
  .p2align 4
  .type cuStreamSynchronize, %function
cuStreamSynchronize:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3296(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3296(%rip)
2:
  pushq $412
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamSynchronize_ptsz
  .p2align 4
  .type cuStreamSynchronize_ptsz, %function
cuStreamSynchronize_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3304(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3304(%rip)
2:
  pushq $413
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWaitEvent
  .p2align 4
  .type cuStreamWaitEvent, %function
cuStreamWaitEvent:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3312(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3312(%rip)
2:
  pushq $414
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWaitEvent_ptsz
  .p2align 4
  .type cuStreamWaitEvent_ptsz, %function
cuStreamWaitEvent_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3320(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3320(%rip)
2:
  pushq $415
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWaitValue32
  .p2align 4
  .type cuStreamWaitValue32, %function
cuStreamWaitValue32:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3328(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3328(%rip)
2:
  pushq $416
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWaitValue32_ptsz
  .p2align 4
  .type cuStreamWaitValue32_ptsz, %function
cuStreamWaitValue32_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3336(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3336(%rip)
2:
  pushq $417
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWaitValue64
  .p2align 4
  .type cuStreamWaitValue64, %function
cuStreamWaitValue64:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3344(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3344(%rip)
2:
  pushq $418
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWaitValue64_ptsz
  .p2align 4
  .type cuStreamWaitValue64_ptsz, %function
cuStreamWaitValue64_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3352(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3352(%rip)
2:
  pushq $419
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWriteValue32
  .p2align 4
  .type cuStreamWriteValue32, %function
cuStreamWriteValue32:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3360(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3360(%rip)
2:
  pushq $420
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWriteValue32_ptsz
  .p2align 4
  .type cuStreamWriteValue32_ptsz, %function
cuStreamWriteValue32_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3368(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3368(%rip)
2:
  pushq $421
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWriteValue64
  .p2align 4
  .type cuStreamWriteValue64, %function
cuStreamWriteValue64:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3376(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3376(%rip)
2:
  pushq $422
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuStreamWriteValue64_ptsz
  .p2align 4
  .type cuStreamWriteValue64_ptsz, %function
cuStreamWriteValue64_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3384(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3384(%rip)
2:
  pushq $423
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuSurfObjectCreate
  .p2align 4
  .type cuSurfObjectCreate, %function
cuSurfObjectCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3392(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3392(%rip)
2:
  pushq $424
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuSurfObjectDestroy
  .p2align 4
  .type cuSurfObjectDestroy, %function
cuSurfObjectDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3400(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3400(%rip)
2:
  pushq $425
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuSurfObjectGetResourceDesc
  .p2align 4
  .type cuSurfObjectGetResourceDesc, %function
cuSurfObjectGetResourceDesc:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3408(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3408(%rip)
2:
  pushq $426
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuSurfRefGetArray
  .p2align 4
  .type cuSurfRefGetArray, %function
cuSurfRefGetArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3416(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3416(%rip)
2:
  pushq $427
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuSurfRefSetArray
  .p2align 4
  .type cuSurfRefSetArray, %function
cuSurfRefSetArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3424(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3424(%rip)
2:
  pushq $428
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexObjectCreate
  .p2align 4
  .type cuTexObjectCreate, %function
cuTexObjectCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3432(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3432(%rip)
2:
  pushq $429
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexObjectDestroy
  .p2align 4
  .type cuTexObjectDestroy, %function
cuTexObjectDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3440(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3440(%rip)
2:
  pushq $430
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexObjectGetResourceDesc
  .p2align 4
  .type cuTexObjectGetResourceDesc, %function
cuTexObjectGetResourceDesc:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3448(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3448(%rip)
2:
  pushq $431
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexObjectGetResourceViewDesc
  .p2align 4
  .type cuTexObjectGetResourceViewDesc, %function
cuTexObjectGetResourceViewDesc:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3456(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3456(%rip)
2:
  pushq $432
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexObjectGetTextureDesc
  .p2align 4
  .type cuTexObjectGetTextureDesc, %function
cuTexObjectGetTextureDesc:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3464(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3464(%rip)
2:
  pushq $433
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefCreate
  .p2align 4
  .type cuTexRefCreate, %function
cuTexRefCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3472(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3472(%rip)
2:
  pushq $434
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefDestroy
  .p2align 4
  .type cuTexRefDestroy, %function
cuTexRefDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3480(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3480(%rip)
2:
  pushq $435
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetAddress
  .p2align 4
  .type cuTexRefGetAddress, %function
cuTexRefGetAddress:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3488(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3488(%rip)
2:
  pushq $436
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetAddressMode
  .p2align 4
  .type cuTexRefGetAddressMode, %function
cuTexRefGetAddressMode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3496(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3496(%rip)
2:
  pushq $437
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetAddress_v2
  .p2align 4
  .type cuTexRefGetAddress_v2, %function
cuTexRefGetAddress_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3504(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3504(%rip)
2:
  pushq $438
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetArray
  .p2align 4
  .type cuTexRefGetArray, %function
cuTexRefGetArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3512(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3512(%rip)
2:
  pushq $439
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetBorderColor
  .p2align 4
  .type cuTexRefGetBorderColor, %function
cuTexRefGetBorderColor:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3520(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3520(%rip)
2:
  pushq $440
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetFilterMode
  .p2align 4
  .type cuTexRefGetFilterMode, %function
cuTexRefGetFilterMode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3528(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3528(%rip)
2:
  pushq $441
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetFlags
  .p2align 4
  .type cuTexRefGetFlags, %function
cuTexRefGetFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3536(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3536(%rip)
2:
  pushq $442
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetFormat
  .p2align 4
  .type cuTexRefGetFormat, %function
cuTexRefGetFormat:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3544(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3544(%rip)
2:
  pushq $443
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetMaxAnisotropy
  .p2align 4
  .type cuTexRefGetMaxAnisotropy, %function
cuTexRefGetMaxAnisotropy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3552(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3552(%rip)
2:
  pushq $444
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetMipmapFilterMode
  .p2align 4
  .type cuTexRefGetMipmapFilterMode, %function
cuTexRefGetMipmapFilterMode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3560(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3560(%rip)
2:
  pushq $445
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetMipmapLevelBias
  .p2align 4
  .type cuTexRefGetMipmapLevelBias, %function
cuTexRefGetMipmapLevelBias:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3568(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3568(%rip)
2:
  pushq $446
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetMipmapLevelClamp
  .p2align 4
  .type cuTexRefGetMipmapLevelClamp, %function
cuTexRefGetMipmapLevelClamp:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3576(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3576(%rip)
2:
  pushq $447
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefGetMipmappedArray
  .p2align 4
  .type cuTexRefGetMipmappedArray, %function
cuTexRefGetMipmappedArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3584(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3584(%rip)
2:
  pushq $448
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetAddress
  .p2align 4
  .type cuTexRefSetAddress, %function
cuTexRefSetAddress:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3592(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3592(%rip)
2:
  pushq $449
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetAddress2D
  .p2align 4
  .type cuTexRefSetAddress2D, %function
cuTexRefSetAddress2D:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3600(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3600(%rip)
2:
  pushq $450
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetAddress2D_v2
  .p2align 4
  .type cuTexRefSetAddress2D_v2, %function
cuTexRefSetAddress2D_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3608(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3608(%rip)
2:
  pushq $451
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetAddress2D_v3
  .p2align 4
  .type cuTexRefSetAddress2D_v3, %function
cuTexRefSetAddress2D_v3:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3616(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3616(%rip)
2:
  pushq $452
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetAddressMode
  .p2align 4
  .type cuTexRefSetAddressMode, %function
cuTexRefSetAddressMode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3624(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3624(%rip)
2:
  pushq $453
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetAddress_v2
  .p2align 4
  .type cuTexRefSetAddress_v2, %function
cuTexRefSetAddress_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3632(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3632(%rip)
2:
  pushq $454
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetArray
  .p2align 4
  .type cuTexRefSetArray, %function
cuTexRefSetArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3640(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3640(%rip)
2:
  pushq $455
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetBorderColor
  .p2align 4
  .type cuTexRefSetBorderColor, %function
cuTexRefSetBorderColor:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3648(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3648(%rip)
2:
  pushq $456
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetFilterMode
  .p2align 4
  .type cuTexRefSetFilterMode, %function
cuTexRefSetFilterMode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3656(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3656(%rip)
2:
  pushq $457
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetFlags
  .p2align 4
  .type cuTexRefSetFlags, %function
cuTexRefSetFlags:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3664(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3664(%rip)
2:
  pushq $458
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetFormat
  .p2align 4
  .type cuTexRefSetFormat, %function
cuTexRefSetFormat:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3672(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3672(%rip)
2:
  pushq $459
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetMaxAnisotropy
  .p2align 4
  .type cuTexRefSetMaxAnisotropy, %function
cuTexRefSetMaxAnisotropy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3680(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3680(%rip)
2:
  pushq $460
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetMipmapFilterMode
  .p2align 4
  .type cuTexRefSetMipmapFilterMode, %function
cuTexRefSetMipmapFilterMode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3688(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3688(%rip)
2:
  pushq $461
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetMipmapLevelBias
  .p2align 4
  .type cuTexRefSetMipmapLevelBias, %function
cuTexRefSetMipmapLevelBias:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3696(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3696(%rip)
2:
  pushq $462
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetMipmapLevelClamp
  .p2align 4
  .type cuTexRefSetMipmapLevelClamp, %function
cuTexRefSetMipmapLevelClamp:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3704(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3704(%rip)
2:
  pushq $463
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuTexRefSetMipmappedArray
  .p2align 4
  .type cuTexRefSetMipmappedArray, %function
cuTexRefSetMipmappedArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3712(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3712(%rip)
2:
  pushq $464
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuThreadExchangeStreamCaptureMode
  .p2align 4
  .type cuThreadExchangeStreamCaptureMode, %function
cuThreadExchangeStreamCaptureMode:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3720(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3720(%rip)
2:
  pushq $465
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuVDPAUCtxCreate
  .p2align 4
  .type cuVDPAUCtxCreate, %function
cuVDPAUCtxCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3728(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3728(%rip)
2:
  pushq $466
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuVDPAUCtxCreate_v2
  .p2align 4
  .type cuVDPAUCtxCreate_v2, %function
cuVDPAUCtxCreate_v2:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3736(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3736(%rip)
2:
  pushq $467
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuVDPAUGetDevice
  .p2align 4
  .type cuVDPAUGetDevice, %function
cuVDPAUGetDevice:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3744(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3744(%rip)
2:
  pushq $468
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuWaitExternalSemaphoresAsync
  .p2align 4
  .type cuWaitExternalSemaphoresAsync, %function
cuWaitExternalSemaphoresAsync:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3752(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3752(%rip)
2:
  pushq $469
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuWaitExternalSemaphoresAsync_ptsz
  .p2align 4
  .type cuWaitExternalSemaphoresAsync_ptsz, %function
cuWaitExternalSemaphoresAsync_ptsz:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3760(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3760(%rip)
2:
  pushq $470
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgApiAttach
  .p2align 4
  .type cudbgApiAttach, %function
cudbgApiAttach:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3768(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3768(%rip)
2:
  pushq $471
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgApiDetach
  .p2align 4
  .type cudbgApiDetach, %function
cudbgApiDetach:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3776(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3776(%rip)
2:
  pushq $472
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgApiInit
  .p2align 4
  .type cudbgApiInit, %function
cudbgApiInit:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3784(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3784(%rip)
2:
  pushq $473
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgGetAPI
  .p2align 4
  .type cudbgGetAPI, %function
cudbgGetAPI:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3792(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3792(%rip)
2:
  pushq $474
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgGetAPIVersion
  .p2align 4
  .type cudbgGetAPIVersion, %function
cudbgGetAPIVersion:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3800(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3800(%rip)
2:
  pushq $475
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgMain
  .p2align 4
  .type cudbgMain, %function
cudbgMain:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3808(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3808(%rip)
2:
  pushq $476
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgReportDriverApiError
  .p2align 4
  .type cudbgReportDriverApiError, %function
cudbgReportDriverApiError:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3816(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3816(%rip)
2:
  pushq $477
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cudbgReportDriverInternalError
  .p2align 4
  .type cudbgReportDriverInternalError, %function
cudbgReportDriverInternalError:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libcuda_so_tramp_table+3824(%rip)
  je 2f
1:
  jmp *_libcuda_so_tramp_table+3824(%rip)
2:
  pushq $478
  .cfi_adjust_cfa_offset 8
  call _libcuda_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

