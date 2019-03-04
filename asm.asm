        .text
        .intel_syntax noprefix
.LBB1_32:
        movabs  rax, 0
        call    rax
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 232
        mov     edx, dword ptr [rsp + 476]
        movabs  r8, 0
        mov     dword ptr [rsp + 36], eax
        call    r8
        movabs  rcx, 0
        vmovss  xmm1, dword ptr [rcx]
        vucomiss        xmm1, xmm0
        seta    r9b
        and     r9b, 1
        movzx   eax, r9b
        mov     dword ptr [rsp + 488], eax
        cmp     dword ptr [rsp + 488], 0
        jne     .LBB1_6
.LBB1_5:
        vxorps  xmm0, xmm0, xmm0
        mov     rax, qword ptr [rsp + 328]
        add     rax, 64
        mov     edx, dword ptr [rsp + 476]
        movabs  rcx, 0
        mov     qword ptr [rsp + 320], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 320]
        vmovss  dword ptr [rsp + 316], xmm0
        call    rax
        vmovss  dword ptr [rsp + 484], xmm1
        vmovss  dword ptr [rsp + 480], xmm0
        vmovss  xmm0, dword ptr [rsp + 480]
        vmovss  xmm1, dword ptr [rsp + 484]
        vmovss  xmm2, dword ptr [rsp + 316]
        vucomiss        xmm2, xmm0
        vmovss  dword ptr [rsp + 312], xmm1
        jae     .LBB1_8
.LBB1_9:
        vxorps  xmm0, xmm0, xmm0
        vmovss  xmm1, dword ptr [rsp + 480]
        vmovss  xmm2, dword ptr [rsp + 484]
        vucomiss        xmm0, xmm2
        vmovss  dword ptr [rsp + 308], xmm1
        jae     .LBB1_8
        vmovss  xmm0, dword ptr [rsp + 480]
        vmovss  xmm1, dword ptr [rsp + 484]
        mov     eax, dword ptr [rsp + 336]
        sub     eax, 1
        vcvtsi2ss       xmm2, xmm2, eax
        vucomiss        xmm0, xmm2
        vmovss  dword ptr [rsp + 304], xmm1
        jae     .LBB1_8
        vmovss  xmm0, dword ptr [rsp + 480]
        vmovss  xmm1, dword ptr [rsp + 484]
        mov     eax, dword ptr [rsp + 340]
        sub     eax, 1
        vcvtsi2ss       xmm2, xmm2, eax
        vucomiss        xmm1, xmm2
        setb    cl
        and     cl, 1
        movzx   eax, cl
        cmp     eax, 0
        sete    cl
        and     cl, 1
        movzx   eax, cl
        mov     dword ptr [rsp + 528], eax
        vmovss  dword ptr [rsp + 300], xmm0
.LBB1_12:
        mov     eax, dword ptr [rsp + 528]
        mov     dword ptr [rsp + 492], eax
        cmp     dword ptr [rsp + 492], 0
        jne     .LBB1_14
        lea     rax, [rsp + 456]
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 120
        mov     edx, dword ptr [rsp + 476]
        movabs  r8, 0
        mov     qword ptr [rsp + 288], rax
        call    r8
        mov     rax, qword ptr [rsp + 328]
        vmovss  xmm2, dword ptr [rax + 344]
        movabs  rcx, 0
        mov     qword ptr [rsp + 280], rcx
        call    rcx
        vmovss  xmm2, dword ptr [rsp + 448]
        vmovss  xmm3, dword ptr [rsp + 452]
        mov     rax, qword ptr [rsp + 328]
        vmovss  xmm4, dword ptr [rax + 344]
        movabs  rcx, 0
        vmovss  xmm5, dword ptr [rcx]
        vsubss  xmm4, xmm5, xmm4
        vmovss  dword ptr [rsp + 276], xmm0
        vmovaps xmm0, xmm2
        vmovss  dword ptr [rsp + 272], xmm1
        vmovaps xmm1, xmm3
        vmovaps xmm2, xmm4
        mov     rcx, qword ptr [rsp + 280]
        call    rcx
        movabs  rax, 0
        vmovss  xmm2, dword ptr [rsp + 276]
        vmovss  dword ptr [rsp + 268], xmm0
        vmovaps xmm0, xmm2
        vmovss  xmm3, dword ptr [rsp + 272]
        vmovss  dword ptr [rsp + 264], xmm1
        vmovaps xmm1, xmm3
        vmovss  xmm2, dword ptr [rsp + 268]
        vmovss  xmm3, dword ptr [rsp + 264]
        call    rax
        vmovss  dword ptr [rsp + 460], xmm1
        vmovss  dword ptr [rsp + 456], xmm0
        movabs  rax, 0
        lea     rcx, [rsp + 456]
        call    rax
        movabs  rax, 0
        vmovss  xmm1, dword ptr [rax]
        vucomiss        xmm1, xmm0
        seta    r9b
        and     r9b, 1
        movzx   edx, r9b
        mov     dword ptr [rsp + 496], edx
        cmp     dword ptr [rsp + 496], 0
        jne     .LBB1_16
.LBB1_15:
        vxorps  xmm0, xmm0, xmm0
        movabs  rax, 0
        lea     rcx, [rsp + 456]
        vmovss  dword ptr [rsp + 260], xmm0
        call    rax
        mov     rax, qword ptr [rsp + 328]
        add     rax, 120
        mov     edx, dword ptr [rsp + 476]
        vmovss  xmm2, dword ptr [rsp + 456]
        vmovss  xmm3, dword ptr [rsp + 460]
        movabs  rcx, 0
        mov     qword ptr [rsp + 248], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 248]
        call    rax
        vmovss  xmm0, dword ptr [rsp + 480]
        vmovss  xmm1, dword ptr [rsp + 484]
        vmovss  xmm2, dword ptr [rsp + 456]
        vmovss  xmm3, dword ptr [rsp + 460]
        movabs  rax, 0
        call    rax
        vmovss  dword ptr [rsp + 468], xmm1
        vmovss  dword ptr [rsp + 464], xmm0
        mov     rax, qword ptr [rsp + 328]
        add     rax, 64
        mov     edx, dword ptr [rsp + 476]
        vmovss  xmm2, dword ptr [rsp + 464]
        vmovss  xmm3, dword ptr [rsp + 468]
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 248]
        call    rax
        vmovss  xmm0, dword ptr [rsp + 464]
        vmovss  xmm1, dword ptr [rsp + 468]
        vmovss  xmm2, dword ptr [rsp + 260]
        vucomiss        xmm2, xmm0
        vmovss  dword ptr [rsp + 244], xmm1
        jae     .LBB1_17
.LBB1_18:
        vxorps  xmm0, xmm0, xmm0
        vmovss  xmm1, dword ptr [rsp + 464]
        vmovss  xmm2, dword ptr [rsp + 468]
        vucomiss        xmm0, xmm2
        vmovss  dword ptr [rsp + 240], xmm1
        jae     .LBB1_17
        vmovss  xmm0, dword ptr [rsp + 464]
        vmovss  xmm1, dword ptr [rsp + 468]
        mov     eax, dword ptr [rsp + 336]
        sub     eax, 1
        vcvtsi2ss       xmm2, xmm2, eax
        vucomiss        xmm0, xmm2
        vmovss  dword ptr [rsp + 236], xmm1
        jae     .LBB1_17
        vmovss  xmm0, dword ptr [rsp + 464]
        vmovss  xmm1, dword ptr [rsp + 468]
        mov     eax, dword ptr [rsp + 340]
        sub     eax, 1
        vcvtsi2ss       xmm2, xmm2, eax
        vucomiss        xmm1, xmm2
        setb    cl
        and     cl, 1
        movzx   eax, cl
        cmp     eax, 0
        sete    cl
        and     cl, 1
        movzx   eax, cl
        mov     dword ptr [rsp + 532], eax
        vmovss  dword ptr [rsp + 232], xmm0
.LBB1_21:
        mov     eax, dword ptr [rsp + 532]
        mov     dword ptr [rsp + 500], eax
        cmp     dword ptr [rsp + 500], 0
        jne     .LBB1_23
        vcvttss2si      eax, dword ptr [rsp + 480]
        mov     dword ptr [rsp + 348], eax
        vcvttss2si      eax, dword ptr [rsp + 484]
        mov     dword ptr [rsp + 352], eax
        vmovss  xmm0, dword ptr [rsp + 480]
        vcvtsi2ss       xmm1, xmm1, dword ptr [rsp + 348]
        vsubss  xmm0, xmm0, xmm1
        vmovss  dword ptr [rsp + 364], xmm0
        vmovss  xmm0, dword ptr [rsp + 484]
        vcvtsi2ss       xmm1, xmm1, dword ptr [rsp + 352]
        vsubss  xmm0, xmm0, xmm1
        vmovss  dword ptr [rsp + 368], xmm0
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 8
        mov     eax, dword ptr [rsp + 348]
        mov     edx, dword ptr [rsp + 352]
        mov     r8d, dword ptr [rsp + 336]
        imul    edx, r8d
        add     eax, edx
        movabs  r9, 0
        mov     qword ptr [rsp + 224], rcx
        mov     edx, eax
        mov     qword ptr [rsp + 216], r9
        call    r9
        vmovss  dword ptr [rsp + 372], xmm0
        mov     eax, dword ptr [rsp + 348]
        mov     edx, dword ptr [rsp + 352]
        inc     edx
        mov     r8d, dword ptr [rsp + 336]
        imul    edx, r8d
        add     eax, edx
        mov     rcx, qword ptr [rsp + 224]
        mov     edx, eax
        mov     r9, qword ptr [rsp + 216]
        call    r9
        vmovss  dword ptr [rsp + 376], xmm0
        mov     eax, dword ptr [rsp + 348]
        mov     edx, dword ptr [rsp + 352]
        mov     r8d, dword ptr [rsp + 336]
        imul    edx, r8d
        mov     ecx, edx
        mov     r9d, eax
        lea     edx, [r9 + rcx + 1]
        mov     rcx, qword ptr [rsp + 224]
        mov     r9, qword ptr [rsp + 216]
        call    r9
        vmovss  dword ptr [rsp + 380], xmm0
        mov     eax, dword ptr [rsp + 348]
        mov     edx, dword ptr [rsp + 352]
        inc     edx
        mov     r8d, dword ptr [rsp + 336]
        imul    edx, r8d
        mov     ecx, edx
        mov     r9d, eax
        lea     edx, [r9 + rcx + 1]
        mov     rcx, qword ptr [rsp + 224]
        mov     r9, qword ptr [rsp + 216]
        call    r9
        vmovss  dword ptr [rsp + 384], xmm0
        vmovss  xmm0, dword ptr [rsp + 372]
        vmovss  xmm1, dword ptr [rsp + 376]
        vaddss  xmm0, xmm0, xmm1
        vmovss  xmm1, dword ptr [rsp + 380]
        vaddss  xmm0, xmm0, xmm1
        vmovss  xmm1, dword ptr [rsp + 384]
        vaddss  xmm0, xmm0, xmm1
        movabs  rcx, 0
        vmovss  xmm1, dword ptr [rcx]
        vmulss  xmm0, xmm0, xmm1
        vmovss  dword ptr [rsp + 388], xmm0
        vmovss  xmm0, dword ptr [rsp + 380]
        vmovss  xmm1, dword ptr [rsp + 372]
        vsubss  xmm0, xmm0, xmm1
        vmovss  xmm1, dword ptr [rsp + 368]
        movabs  rcx, 0
        vmovss  xmm2, dword ptr [rcx]
        vsubss  xmm3, xmm2, xmm1
        vmovss  xmm4, dword ptr [rsp + 384]
        vmovss  xmm5, dword ptr [rsp + 376]
        vsubss  xmm4, xmm4, xmm5
        vmulss  xmm1, xmm4, xmm1
        vfmadd213ss     xmm3, xmm0, xmm1
        vmovss  dword ptr [rsp + 448], xmm3
        vmovss  xmm0, dword ptr [rsp + 376]
        vmovss  xmm1, dword ptr [rsp + 372]
        vsubss  xmm0, xmm0, xmm1
        vmovss  xmm1, dword ptr [rsp + 364]
        vsubss  xmm2, xmm2, xmm1
        vmovss  xmm3, dword ptr [rsp + 384]
        vmovss  xmm4, dword ptr [rsp + 380]
        vsubss  xmm3, xmm3, xmm4
        vmulss  xmm1, xmm3, xmm1
        vfmadd213ss     xmm2, xmm0, xmm1
        vmovss  dword ptr [rsp + 452], xmm2
        vcvttss2si      eax, dword ptr [rsp + 464]
        mov     dword ptr [rsp + 356], eax
        vmovss  xmm0, dword ptr [rsp + 464]
        vmovss  xmm1, dword ptr [rsp + 468]
        vcvttss2si      eax, xmm1
        mov     dword ptr [rsp + 360], eax
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 8
        mov     eax, dword ptr [rsp + 356]
        mov     edx, dword ptr [rsp + 360]
        imul    edx, dword ptr [rsp + 336]
        add     eax, edx
        movabs  r9, 0
        mov     edx, eax
        vmovss  dword ptr [rsp + 212], xmm0
        call    r9
        vmovss  dword ptr [rsp + 400], xmm0
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 8
        mov     eax, dword ptr [rsp + 356]
        mov     edx, dword ptr [rsp + 360]
        add     edx, 1
        imul    edx, dword ptr [rsp + 336]
        add     eax, edx
        movabs  r9, 0
        mov     edx, eax
        call    r9
        vmovss  dword ptr [rsp + 404], xmm0
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 8
        mov     eax, dword ptr [rsp + 356]
        add     eax, 1
        mov     edx, dword ptr [rsp + 360]
        imul    edx, dword ptr [rsp + 336]
        add     eax, edx
        movabs  r9, 0
        mov     edx, eax
        call    r9
        vmovss  dword ptr [rsp + 408], xmm0
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 8
        mov     eax, dword ptr [rsp + 356]
        add     eax, 1
        mov     edx, dword ptr [rsp + 360]
        add     edx, 1
        imul    edx, dword ptr [rsp + 336]
        add     eax, edx
        movabs  r9, 0
        mov     edx, eax
        call    r9
        movabs  rcx, 0
        vmovss  xmm1, dword ptr [rcx]
        vmovss  dword ptr [rsp + 412], xmm0
        vmovss  xmm0, dword ptr [rsp + 400]
        vaddss  xmm0, xmm0, dword ptr [rsp + 404]
        vaddss  xmm0, xmm0, dword ptr [rsp + 408]
        vaddss  xmm0, xmm0, dword ptr [rsp + 412]
        vmulss  xmm0, xmm0, xmm1
        vmovss  dword ptr [rsp + 416], xmm0
        vmovss  xmm0, dword ptr [rsp + 416]
        vsubss  xmm0, xmm0, dword ptr [rsp + 388]
        vmovss  dword ptr [rsp + 420], xmm0
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 288
        mov     edx, dword ptr [rsp + 476]
        movabs  r9, 0
        call    r9
        vmovss  dword ptr [rsp + 424], xmm0
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 176
        mov     edx, dword ptr [rsp + 476]
        movabs  r9, 0
        call    r9
        vmovss  dword ptr [rsp + 432], xmm0
        mov     rcx, qword ptr [rsp + 328]
        add     rcx, 232
        mov     edx, dword ptr [rsp + 476]
        movabs  r9, 0
        call    r9
        movabs  rcx, 0
        vmovss  xmm1, dword ptr [rcx]
        movabs  rcx, 0
        vmovss  xmm2, dword ptr [rcx]
        vmovss  dword ptr [rsp + 440], xmm0
        vmovss  xmm0, dword ptr [rsp + 440]
        mov     rcx, qword ptr [rsp + 328]
        vsubss  xmm2, xmm2, dword ptr [rcx + 360]
        vmulss  xmm0, xmm0, xmm2
        vmovss  dword ptr [rsp + 444], xmm0
        vucomiss        xmm1, dword ptr [rsp + 444]
        seta    r10b
        and     r10b, 1
        movzx   eax, r10b
        mov     dword ptr [rsp + 504], eax
        cmp     dword ptr [rsp + 504], 0
        jne     .LBB1_25
.LBB1_24:
        vxorps  xmm0, xmm0, xmm0
        vmovss  xmm1, dword ptr [rsp + 420]
        vucomiss        xmm1, xmm0
        seta    al
        and     al, 1
        movzx   ecx, al
        mov     dword ptr [rsp + 508], ecx
        cmp     dword ptr [rsp + 508], 0
        jne     .LBB1_27
.LBB1_26:
        vmovss  xmm0, dword ptr [rsp + 420]
        vmovd   eax, xmm0
        xor     eax, 2147483648
        vmovd   xmm0, eax
        mov     rcx, qword ptr [rsp + 328]
        vmovss  xmm1, dword ptr [rcx + 368]
        movabs  rdx, 0
        call    rdx
        vmulss  xmm0, xmm0, dword ptr [rsp + 432]
        vmulss  xmm0, xmm0, dword ptr [rsp + 440]
        mov     rcx, qword ptr [rsp + 328]
        vmulss  xmm0, xmm0, dword ptr [rcx + 348]
        vmovss  dword ptr [rsp + 512], xmm0
        vmovss  xmm0, dword ptr [rsp + 424]
        vucomiss        xmm0, dword ptr [rsp + 512]
        seta    r8b
        and     r8b, 1
        movzx   eax, r8b
        mov     dword ptr [rsp + 516], eax
        cmp     dword ptr [rsp + 516], 0
        jne     .LBB1_30
.LBB1_29:
        vmovss  xmm0, dword ptr [rsp + 512]
        vsubss  xmm0, xmm0, dword ptr [rsp + 424]
        mov     rax, qword ptr [rsp + 328]
        vmulss  xmm0, xmm0, dword ptr [rax + 356]
        vmovss  xmm1, dword ptr [rsp + 420]
        vmovd   ecx, xmm1
        xor     ecx, 2147483648
        vmovd   xmm1, ecx
        movabs  rdx, 0
        call    rdx
        movabs  rax, 0
        vmovss  xmm1, dword ptr [rax]
        movabs  rax, 0
        vmovss  xmm2, dword ptr [rax]
        vmulss  xmm0, xmm0, xmm2
        vmovss  dword ptr [rsp + 428], xmm0
        mov     rax, qword ptr [rsp + 328]
        add     rax, 8
        mov     ecx, dword ptr [rsp + 348]
        mov     r8d, dword ptr [rsp + 352]
        imul    r8d, dword ptr [rsp + 336]
        add     ecx, r8d
        vmovss  xmm0, dword ptr [rsp + 372]
        vmovss  xmm2, dword ptr [rsp + 428]
        vsubss  xmm1, xmm1, dword ptr [rsp + 364]
        vmulss  xmm1, xmm2, xmm1
        vsubss  xmm2, xmm0, xmm1
        movabs  rdx, 0
        mov     dword ptr [rsp + 104], ecx
        mov     rcx, rax
        mov     r8d, dword ptr [rsp + 104]
        mov     qword ptr [rsp + 96], rdx
        mov     edx, r8d
        mov     rax, qword ptr [rsp + 96]
        call    rax
        mov     rax, qword ptr [rsp + 328]
        add     rax, 8
        mov     edx, dword ptr [rsp + 348]
        add     edx, 1
        mov     r8d, dword ptr [rsp + 352]
        imul    r8d, dword ptr [rsp + 336]
        add     edx, r8d
        vmovss  xmm0, dword ptr [rsp + 380]
        vmovss  xmm1, dword ptr [rsp + 428]
        vmulss  xmm1, xmm1, dword ptr [rsp + 364]
        vsubss  xmm2, xmm0, xmm1
        movabs  rcx, 0
        mov     qword ptr [rsp + 88], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 88]
        call    rax
        movabs  rax, 0
        vmovss  xmm0, dword ptr [rax]
        mov     rax, qword ptr [rsp + 328]
        add     rax, 8
        mov     edx, dword ptr [rsp + 348]
        mov     r8d, dword ptr [rsp + 352]
        add     r8d, 1
        imul    r8d, dword ptr [rsp + 336]
        add     edx, r8d
        vmovss  xmm1, dword ptr [rsp + 376]
        vmovss  xmm2, dword ptr [rsp + 428]
        vsubss  xmm0, xmm0, dword ptr [rsp + 368]
        vmulss  xmm0, xmm2, xmm0
        vsubss  xmm2, xmm1, xmm0
        movabs  rcx, 0
        mov     qword ptr [rsp + 80], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 80]
        call    rax
        mov     rax, qword ptr [rsp + 328]
        add     rax, 8
        mov     edx, dword ptr [rsp + 348]
        add     edx, 1
        mov     r8d, dword ptr [rsp + 352]
        add     r8d, 1
        imul    r8d, dword ptr [rsp + 336]
        add     edx, r8d
        vmovss  xmm0, dword ptr [rsp + 384]
        vmovss  xmm1, dword ptr [rsp + 428]
        vmulss  xmm1, xmm1, dword ptr [rsp + 368]
        vsubss  xmm2, xmm0, xmm1
        movabs  rcx, 0
        mov     qword ptr [rsp + 72], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 72]
        call    rax
        movabs  rax, 0
        vmovss  xmm0, dword ptr [rax]
        vmovss  xmm1, dword ptr [rsp + 424]
        vmulss  xmm0, xmm0, dword ptr [rsp + 428]
        vaddss  xmm0, xmm1, xmm0
        vmovss  dword ptr [rsp + 424], xmm0
.LBB1_31:
.LBB1_28:
        vmovss  xmm0, dword ptr [rsp + 432]
        vmulss  xmm0, xmm0, dword ptr [rsp + 432]
        vmovss  xmm1, dword ptr [rsp + 420]
        mov     rax, qword ptr [rsp + 328]
        vmulss  xmm1, xmm1, dword ptr [rax + 376]
        vsubss  xmm0, xmm0, xmm1
        movabs  rcx, 0
        call    rcx
        vcvtss2sd       xmm0, xmm1, xmm0
        movabs  rax, 0
        call    rax
        vcvtsd2ss       xmm0, xmm1, xmm0
        vmovss  dword ptr [rsp + 436], xmm0
        mov     rax, qword ptr [rsp + 328]
        add     rax, 176
        mov     edx, dword ptr [rsp + 476]
        vmovss  xmm2, dword ptr [rsp + 436]
        movabs  rcx, 0
        mov     qword ptr [rsp + 128], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 128]
        call    rax
        mov     rax, qword ptr [rsp + 328]
        add     rax, 232
        mov     edx, dword ptr [rsp + 476]
        vmovss  xmm2, dword ptr [rsp + 444]
        movabs  rcx, 0
        mov     qword ptr [rsp + 120], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 120]
        call    rax
        mov     rax, qword ptr [rsp + 328]
        add     rax, 288
        mov     edx, dword ptr [rsp + 476]
        vmovss  xmm2, dword ptr [rsp + 424]
        movabs  rcx, 0
        mov     qword ptr [rsp + 112], rcx
        mov     rcx, rax
        mov     rax, qword ptr [rsp + 112]
        call    rax
        movabs  rax, 0
        call    rax
        mov     dword ptr [rsp + 108], eax
        jmp     .LBB1_7
