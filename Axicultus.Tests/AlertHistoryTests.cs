using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Xunit;

namespace Axicultus.Tests;

public class AlertHistoryTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(19, false)]
    [InlineData(20, false)]
    [InlineData(25, false)]
    [InlineData(20, true)]
    [InlineData(25, true)]
    public void SendingAnAlertKeepsTheLatestTwentyInTheListAndFile(int existingCount, bool repeatLatest)
    {
        string[] existing = Enumerable.Range(1, existingCount).Reverse().Select(i => $"알림 {i}").ToArray();
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(path, existing, Encoding.Unicode);
            using var history = new ComboBox();
            history.Items.AddRange(existing);
            history.Text = repeatLatest ? existing[0] : "새 알림";

            Gf.RecordAlertHistory(history, path);

            var expected = (repeatLatest ? existing : existing.Prepend("새 알림")).Take(20).ToArray();
            Assert.Equal(expected, history.Items.Cast<string>().ToArray());
            Assert.Equal(expected, File.ReadAllLines(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
