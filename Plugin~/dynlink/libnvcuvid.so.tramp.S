/*
 * Copyright 2018-2020 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .data

  .globl _libnvcuvid_so_tramp_table
  .hidden _libnvcuvid_so_tramp_table
  .align 8
_libnvcuvid_so_tramp_table:
  .zero 248

  .text

  .globl _libnvcuvid_so_tramp_resolve
  .hidden _libnvcuvid_so_tramp_resolve

  .globl _libnvcuvid_so_save_regs_and_resolve
  .hidden _libnvcuvid_so_save_regs_and_resolve
  .type _libnvcuvid_so_save_regs_and_resolve, %function
_libnvcuvid_so_save_regs_and_resolve:
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
  call _libnvcuvid_so_tramp_resolve

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

  .globl __std_1U4S4U_X02
  .p2align 4
  .type __std_1U4S4U_X02, %function
__std_1U4S4U_X02:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+0(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+0(%rip)
2:
  pushq $0
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl __std_2U4S4U_X08
  .p2align 4
  .type __std_2U4S4U_X08, %function
__std_2U4S4U_X08:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+8(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+8(%rip)
2:
  pushq $1
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl __std_4U4S4U_X04
  .p2align 4
  .type __std_4U4S4U_X04, %function
__std_4U4S4U_X04:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+16(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+16(%rip)
2:
  pushq $2
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidConvertYUVToRGB
  .p2align 4
  .type cuvidConvertYUVToRGB, %function
cuvidConvertYUVToRGB:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+24(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+24(%rip)
2:
  pushq $3
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidConvertYUVToRGBArray
  .p2align 4
  .type cuvidConvertYUVToRGBArray, %function
cuvidConvertYUVToRGBArray:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+32(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+32(%rip)
2:
  pushq $4
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCreateDecoder
  .p2align 4
  .type cuvidCreateDecoder, %function
cuvidCreateDecoder:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+40(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+40(%rip)
2:
  pushq $5
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCreateVideoParser
  .p2align 4
  .type cuvidCreateVideoParser, %function
cuvidCreateVideoParser:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+48(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+48(%rip)
2:
  pushq $6
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCreateVideoSource
  .p2align 4
  .type cuvidCreateVideoSource, %function
cuvidCreateVideoSource:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+56(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+56(%rip)
2:
  pushq $7
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCreateVideoSourceW
  .p2align 4
  .type cuvidCreateVideoSourceW, %function
cuvidCreateVideoSourceW:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+64(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+64(%rip)
2:
  pushq $8
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCtxLock
  .p2align 4
  .type cuvidCtxLock, %function
cuvidCtxLock:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+72(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+72(%rip)
2:
  pushq $9
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCtxLockCreate
  .p2align 4
  .type cuvidCtxLockCreate, %function
cuvidCtxLockCreate:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+80(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+80(%rip)
2:
  pushq $10
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCtxLockDestroy
  .p2align 4
  .type cuvidCtxLockDestroy, %function
cuvidCtxLockDestroy:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+88(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+88(%rip)
2:
  pushq $11
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidCtxUnlock
  .p2align 4
  .type cuvidCtxUnlock, %function
cuvidCtxUnlock:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+96(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+96(%rip)
2:
  pushq $12
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidDecodePicture
  .p2align 4
  .type cuvidDecodePicture, %function
cuvidDecodePicture:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+104(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+104(%rip)
2:
  pushq $13
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidDestroyDecoder
  .p2align 4
  .type cuvidDestroyDecoder, %function
cuvidDestroyDecoder:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+112(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+112(%rip)
2:
  pushq $14
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidDestroyVideoParser
  .p2align 4
  .type cuvidDestroyVideoParser, %function
cuvidDestroyVideoParser:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+120(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+120(%rip)
2:
  pushq $15
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidDestroyVideoSource
  .p2align 4
  .type cuvidDestroyVideoSource, %function
cuvidDestroyVideoSource:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+128(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+128(%rip)
2:
  pushq $16
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidGetDecodeStatus
  .p2align 4
  .type cuvidGetDecodeStatus, %function
cuvidGetDecodeStatus:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+136(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+136(%rip)
2:
  pushq $17
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidGetDecoderCaps
  .p2align 4
  .type cuvidGetDecoderCaps, %function
cuvidGetDecoderCaps:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+144(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+144(%rip)
2:
  pushq $18
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidGetSourceAudioFormat
  .p2align 4
  .type cuvidGetSourceAudioFormat, %function
cuvidGetSourceAudioFormat:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+152(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+152(%rip)
2:
  pushq $19
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidGetSourceVideoFormat
  .p2align 4
  .type cuvidGetSourceVideoFormat, %function
cuvidGetSourceVideoFormat:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+160(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+160(%rip)
2:
  pushq $20
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidGetVideoSourceState
  .p2align 4
  .type cuvidGetVideoSourceState, %function
cuvidGetVideoSourceState:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+168(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+168(%rip)
2:
  pushq $21
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidMapVideoFrame
  .p2align 4
  .type cuvidMapVideoFrame, %function
cuvidMapVideoFrame:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+176(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+176(%rip)
2:
  pushq $22
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidMapVideoFrame64
  .p2align 4
  .type cuvidMapVideoFrame64, %function
cuvidMapVideoFrame64:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+184(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+184(%rip)
2:
  pushq $23
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidParseVideoData
  .p2align 4
  .type cuvidParseVideoData, %function
cuvidParseVideoData:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+192(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+192(%rip)
2:
  pushq $24
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidPrivateOp
  .p2align 4
  .type cuvidPrivateOp, %function
cuvidPrivateOp:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+200(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+200(%rip)
2:
  pushq $25
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidReconfigureDecoder
  .p2align 4
  .type cuvidReconfigureDecoder, %function
cuvidReconfigureDecoder:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+208(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+208(%rip)
2:
  pushq $26
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidSetVideoSourceState
  .p2align 4
  .type cuvidSetVideoSourceState, %function
cuvidSetVideoSourceState:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+216(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+216(%rip)
2:
  pushq $27
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidUnmapVideoFrame
  .p2align 4
  .type cuvidUnmapVideoFrame, %function
cuvidUnmapVideoFrame:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+224(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+224(%rip)
2:
  pushq $28
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

/*
 * Copyright 2018-2019 Yury Gribov
 *
 * The MIT License (MIT)
 *
 * Use of this source code is governed by MIT license that can be
 * found in the LICENSE.txt file.
 */

  .globl cuvidUnmapVideoFrame64
  .p2align 4
  .type cuvidUnmapVideoFrame64, %function
cuvidUnmapVideoFrame64:
  .cfi_startproc
  // Intel opt. manual says to
  // "make the fall-through code following a conditional branch be the likely target for a branch with a forward target"
  // to hint static predictor.
  cmpq $0, _libnvcuvid_so_tramp_table+232(%rip)
  je 2f
1:
  jmp *_libnvcuvid_so_tramp_table+232(%rip)
2:
  pushq $29
  .cfi_adjust_cfa_offset 8
  call _libnvcuvid_so_save_regs_and_resolve
  addq $8, %rsp
  .cfi_adjust_cfa_offset -8
  jmp 1b
  .cfi_endproc

