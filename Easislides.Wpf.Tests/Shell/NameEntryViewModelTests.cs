using System;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 재사용 이름 입력/변경 다이얼로그 VM 검증 — 레거시 FrmBibleRename·FrmUpdateFileName(이름 변경)의 핵심 규칙
/// (빈 이름 거부 / 기존 이름과 중복 거부(대소문자 무시) / 변경 없음 허용)을 UI 없이 검증.
/// </summary>
public sealed class NameEntryViewModelTests
{
    private static NameEntryViewModel Create(string original = "주일오전", params string[] existing)
        => new("예배 순서 이름 변경", "새 이름을 입력하세요", original, existing);

    [Fact]
    public void Initial_NameEqualsOriginal_AndConfirmable()
    {
        var sut = Create("주일오전", "주일오전", "수요예배");

        sut.Name.Should().Be("주일오전", "처음엔 기존 이름이 채워져 있다");
        sut.HasError.Should().BeFalse();
        sut.CanConfirm.Should().BeTrue("변경 없이 확인(닫기)도 허용");
        sut.Result.Should().Be("주일오전");
    }

    [Fact]
    public void EmptyName_IsRejected()
    {
        var sut = Create("주일오전", "주일오전");

        sut.Name = "   ";

        sut.HasError.Should().BeTrue();
        sut.ErrorMessage.Should().Contain("이름");
        sut.CanConfirm.Should().BeFalse("빈 이름은 확인 불가");
    }

    [Fact]
    public void DuplicateName_IsRejected_CaseInsensitive()
    {
        var sut = Create("주일오전", "주일오전", "Special");

        sut.Name = "special"; // 대소문자만 다른 기존 이름

        sut.HasError.Should().BeTrue();
        sut.ErrorMessage.Should().Contain("이미");
        sut.CanConfirm.Should().BeFalse("기존 이름과 중복(대소문자 무시)은 거부");
    }

    [Fact]
    public void NewUniqueName_IsAccepted_AndTrimmed()
    {
        var sut = Create("주일오전", "주일오전", "수요예배");

        sut.Name = "  특별새벽기도  ";

        sut.HasError.Should().BeFalse();
        sut.CanConfirm.Should().BeTrue();
        sut.Result.Should().Be("특별새벽기도", "앞뒤 공백은 제거해 반환");
    }

    [Fact]
    public void UnchangedName_DoesNotCollideWithItself()
    {
        // 자기 자신과는 중복 판정하지 않는다(기존 이름은 후보 목록에 있어도 통과).
        var sut = Create("주일오전", "주일오전", "수요예배");

        sut.Name = "주일오전";

        sut.CanConfirm.Should().BeTrue("자기 이름 그대로는 중복이 아님");
        sut.HasError.Should().BeFalse();
    }

    [Fact]
    public void Title_And_Prompt_AreExposed_ForReuse()
    {
        var sut = new NameEntryViewModel("폴더 이름 변경", "폴더의 새 이름:", "찬양", Array.Empty<string>());

        sut.Title.Should().Be("폴더 이름 변경");
        sut.Prompt.Should().Be("폴더의 새 이름:");
    }
}
