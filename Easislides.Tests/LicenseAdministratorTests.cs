using System.Windows.Forms;
using Xunit;

namespace Easislides.Tests;

[CollectionDefinition("License administrators", DisableParallelization = true)]
public sealed class LicenseAdministratorCollection { }

[Collection("License administrators")]
public class LicenseAdministratorTests
{
    [Fact]
    public void UnknownAdministratorNamesCanBeReadFromAnExistingSong()
    {
        string primary = "Unregistered primary";
        string secondary = "Unregistered secondary";
        Gf.LoadLicAdminDisplayInfo(ref primary, ref secondary);
        Assert.Equal("Unregistered primary", primary);
        Assert.Equal("Unregistered secondary", secondary);
    }

    [Fact]
    public void LastTwoCustomAdministratorsAreSelectableAndResolveToTheirLicenseText()
    {
        var administrators = Gf.LicenseAdministrators;
        var previous7 = administrators[7];
        var previous8 = administrators[8];
        using var primaryChoices = new ComboBox();
        using var secondaryChoices = new ComboBox();
        try
        {
            administrators[7] = new() { Name = "Seventh", Number = "77", DisplayText = "Seventh#77" };
            administrators[8] = new() { Name = "Eighth", Number = "88", DisplayText = "Eighth#88" };
            Gf.PopulateLicenseAdministratorChoices(primaryChoices, secondaryChoices);
            foreach (var choices in new[] { primaryChoices, secondaryChoices })
            {
                Assert.Equal("Seventh", choices.Items[choices.Items.Count - 2]);
                Assert.Equal("Eighth", choices.Items[choices.Items.Count - 1]);
            }
            string primary = "Seventh";
            string secondary = "Eighth";
            Gf.LoadLicAdminDisplayInfo(ref primary, ref secondary);
            Assert.Equal("Seventh#77", primary);
            Assert.Equal("Eighth#88", secondary);
        }
        finally
        {
            administrators[7] = previous7;
            administrators[8] = previous8;
        }
    }
}
