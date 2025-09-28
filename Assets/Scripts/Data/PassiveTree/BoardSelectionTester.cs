using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Test script to demonstrate board selection restrictions
    /// This can be used to verify the board selection system is working correctly
    /// </summary>
    public class BoardSelectionTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool debugMode = true;
        
        [Header("Test Data")]
        [SerializeField] private List<BoardData> testBoards = new List<BoardData>();
        
        void Start()
        {
            if (runTestsOnStart)
            {
                RunBoardSelectionTests();
            }
        }
        
        /// <summary>
        /// Run comprehensive tests for board selection restrictions
        /// </summary>
        [ContextMenu("Run Board Selection Tests")]
        public void RunBoardSelectionTests()
        {
            Debug.Log("=== BOARD SELECTION TESTS START ===");
            
            var tracker = BoardSelectionTracker.Instance;
            if (tracker == null)
            {
                Debug.LogError("BoardSelectionTracker not found! Make sure it's in the scene.");
                return;
            }
            
            // Clear any existing selections
            tracker.ClearAllSelections();
            
            // Test 1: Basic board selection
            TestBasicBoardSelection(tracker);
            
            // Test 2: Duplicate prevention
            TestDuplicatePrevention(tracker);
            
            // Test 3: Tier-based theme restrictions
            TestTierBasedRestrictions(tracker);
            
            // Test 4: Available board filtering
            TestAvailableBoardFiltering(tracker);
            
            // Test 5: Board registration and recognition
            TestBoardRegistration(tracker);
            
            // Test 6: Duplicate theme prevention
            TestDuplicateThemePrevention(tracker);
            
            // Test 7: Dynamic UI filtering
            TestDynamicUIFiltering(tracker);
            
            Debug.Log("=== BOARD SELECTION TESTS COMPLETE ===");
        }
        
        /// <summary>
        /// Test basic board selection functionality
        /// </summary>
        private void TestBasicBoardSelection(BoardSelectionTracker tracker)
        {
            Debug.Log("--- Test 1: Basic Board Selection ---");
            
            if (testBoards.Count == 0)
            {
                Debug.LogWarning("No test boards assigned. Skipping basic selection test.");
                return;
            }
            
            var testBoard = testBoards[0];
            bool success = tracker.SelectBoard(testBoard, 1, testBoard.BoardTheme);
            
            if (success)
            {
                Debug.Log($"✓ Successfully selected board: {testBoard.BoardName}");
                Debug.Log($"✓ Board is selected: {tracker.IsBoardSelected(testBoard)}");
                Debug.Log($"✓ Theme is selected for tier 1: {tracker.IsThemeSelectedForTier(1, testBoard.BoardTheme)}");
            }
            else
            {
                Debug.LogError($"✗ Failed to select board: {testBoard.BoardName}");
            }
        }
        
        /// <summary>
        /// Test duplicate board prevention
        /// </summary>
        private void TestDuplicatePrevention(BoardSelectionTracker tracker)
        {
            Debug.Log("--- Test 2: Duplicate Prevention ---");
            
            if (testBoards.Count == 0)
            {
                Debug.LogWarning("No test boards assigned. Skipping duplicate prevention test.");
                return;
            }
            
            var testBoard = testBoards[0];
            
            // Try to select the same board again
            bool duplicateSuccess = tracker.SelectBoard(testBoard, 1, testBoard.BoardTheme);
            
            if (!duplicateSuccess)
            {
                Debug.Log($"✓ Successfully prevented duplicate selection of: {testBoard.BoardName}");
            }
            else
            {
                Debug.LogError($"✗ Failed to prevent duplicate selection of: {testBoard.BoardName}");
            }
        }
        
        /// <summary>
        /// Test tier-based theme restrictions
        /// </summary>
        private void TestTierBasedRestrictions(BoardSelectionTracker tracker)
        {
            Debug.Log("--- Test 3: Tier-Based Theme Restrictions ---");
            
            if (testBoards.Count < 2)
            {
                Debug.LogWarning("Need at least 2 test boards for tier-based testing. Skipping test.");
                return;
            }
            
            var board1 = testBoards[0];
            var board2 = testBoards[1];
            
            // Select board1 for tier 1
            bool success1 = tracker.SelectBoard(board1, 1, board1.BoardTheme);
            Debug.Log($"Selected {board1.BoardName} for tier 1: {success1}");
            
            // Try to select board2 with same theme for tier 1 (should fail)
            bool success2 = tracker.SelectBoard(board2, 1, board1.BoardTheme);
            Debug.Log($"Tried to select {board2.BoardName} with same theme for tier 1: {success2}");
            
            if (!success2)
            {
                Debug.Log($"✓ Successfully prevented same theme selection for tier 1");
            }
            else
            {
                Debug.LogError($"✗ Failed to prevent same theme selection for tier 1");
            }
            
            // Try to select board2 for tier 2 (should succeed)
            bool success3 = tracker.SelectBoard(board2, 2, board2.BoardTheme);
            Debug.Log($"Selected {board2.BoardName} for tier 2: {success3}");
            
            if (success3)
            {
                Debug.Log($"✓ Successfully allowed different tier selection");
            }
            else
            {
                Debug.LogError($"✗ Failed to allow different tier selection");
            }
        }
        
        /// <summary>
        /// Test available board filtering
        /// </summary>
        private void TestAvailableBoardFiltering(BoardSelectionTracker tracker)
        {
            Debug.Log("--- Test 4: Available Board Filtering ---");
            
            if (testBoards.Count == 0)
            {
                Debug.LogWarning("No test boards assigned. Skipping filtering test.");
                return;
            }
            
            // Get available boards for tier 1
            var availableBoards = tracker.GetAvailableBoards(testBoards, 1, BoardTheme.Fire);
            Debug.Log($"Available boards for tier 1, Fire theme: {availableBoards.Count}");
            
            foreach (var board in availableBoards)
            {
                Debug.Log($"  - {board.BoardName} ({board.BoardTheme})");
            }
            
            // Get all available boards
            var allAvailable = tracker.GetAvailableBoards(testBoards);
            Debug.Log($"All available boards: {allAvailable.Count}");
            
            foreach (var board in allAvailable)
            {
                Debug.Log($"  - {board.BoardName} ({board.BoardTheme})");
            }
        }
        
        /// <summary>
        /// Test board registration and recognition
        /// </summary>
        private void TestBoardRegistration(BoardSelectionTracker tracker)
        {
            Debug.Log("--- Test 5: Board Registration and Recognition ---");
            
            if (testBoards.Count == 0)
            {
                Debug.LogWarning("No test boards assigned. Skipping registration test.");
                return;
            }
            
            var testBoard = testBoards[0];
            string testBoardName = "TestBoard_" + System.DateTime.Now.Ticks;
            
            // Test board registration
            tracker.RegisterCreatedBoard(testBoardName, testBoard, 1, testBoard.BoardTheme);
            Debug.Log($"Board registration attempted for: {testBoardName}");
            
            // Test if board is now recognized as selected
            bool isSelected = tracker.IsBoardSelected(testBoardName);
            Debug.Log($"Board is recognized as selected: {isSelected}");
            
            // Test if board data can be retrieved
            var retrievedBoard = tracker.GetSelectedBoard(1, testBoard.BoardTheme);
            Debug.Log($"Retrieved board: {retrievedBoard?.BoardName}");
            
            // Test board unregistration
            tracker.UnregisterBoard(testBoardName);
            bool isStillSelected = tracker.IsBoardSelected(testBoardName);
            Debug.Log($"Board is still selected after unregistration: {isStillSelected}");
            
            if (isSelected && retrievedBoard != null && !isStillSelected)
            {
                Debug.Log($"✓ Board registration and recognition test passed");
            }
            else
            {
                Debug.LogError($"✗ Board registration and recognition test failed");
            }
        }
        
        /// <summary>
        /// Test duplicate theme prevention
        /// </summary>
        private void TestDuplicateThemePrevention(BoardSelectionTracker tracker)
        {
            Debug.Log("--- Test 6: Duplicate Theme Prevention ---");
            
            if (testBoards.Count < 2)
            {
                Debug.LogWarning("Need at least 2 test boards for duplicate theme testing. Skipping test.");
                return;
            }
            
            var board1 = testBoards[0];
            var board2 = testBoards[1];
            
            // Create test board names that would trigger theme detection
            string frozenMastery1 = "ExtensionBoard_Frozen Mastery_1_0";
            string frozenMastery2 = "ExtensionBoard_Frozen Mastery_0_-1";
            
            // Test that we can create the first board
            bool canCreateFirst = tracker.CanCreateBoard(frozenMastery1, 1, BoardTheme.Cold);
            Debug.Log($"Can create first Frozen Mastery board: {canCreateFirst}");
            
            // Register the first board
            tracker.RegisterCreatedBoard(frozenMastery1, board1, 1, BoardTheme.Cold);
            Debug.Log($"Registered first Frozen Mastery board");
            
            // Test that we cannot create a second board with the same theme
            bool canCreateSecond = tracker.CanCreateBoard(frozenMastery2, 1, BoardTheme.Cold);
            Debug.Log($"Can create second Frozen Mastery board: {canCreateSecond}");
            
            // Test that the theme is marked as selected for the tier
            bool themeSelected = tracker.IsThemeSelectedForTier(1, BoardTheme.Cold);
            Debug.Log($"Cold theme is selected for tier 1: {themeSelected}");
            
            if (canCreateFirst && !canCreateSecond && themeSelected)
            {
                Debug.Log($"✓ Duplicate theme prevention test passed");
            }
            else
            {
                Debug.LogError($"✗ Duplicate theme prevention test failed");
            }
        }
        
        /// <summary>
        /// Test dynamic UI filtering
        /// </summary>
        private void TestDynamicUIFiltering(BoardSelectionTracker tracker)
        {
            Debug.Log("--- Test 7: Dynamic UI Filtering ---");
            
            if (testBoards.Count == 0)
            {
                Debug.LogWarning("No test boards assigned. Skipping UI filtering test.");
                return;
            }
            
            // Test available boards before any selection
            var availableBoardsBefore = tracker.GetAvailableBoards(testBoards);
            Debug.Log($"Available boards before selection: {availableBoardsBefore.Count}");
            
            // Select a board
            var testBoard = testBoards[0];
            bool selectionSuccess = tracker.SelectBoard(testBoard, 1, testBoard.BoardTheme);
            Debug.Log($"Board selection success: {selectionSuccess}");
            
            // Test available boards after selection
            var availableBoardsAfter = tracker.GetAvailableBoards(testBoards);
            Debug.Log($"Available boards after selection: {availableBoardsAfter.Count}");
            
            // Test that the selected board is no longer available
            bool selectedBoardStillAvailable = availableBoardsAfter.Contains(testBoard);
            Debug.Log($"Selected board still available: {selectedBoardStillAvailable}");
            
            // Test that boards with the same theme are no longer available
            var sameThemeBoards = availableBoardsAfter.Where(b => b.BoardTheme == testBoard.BoardTheme).ToList();
            Debug.Log($"Same theme boards still available: {sameThemeBoards.Count}");
            
            if (selectionSuccess && !selectedBoardStillAvailable && sameThemeBoards.Count == 0)
            {
                Debug.Log($"✓ Dynamic UI filtering test passed");
            }
            else
            {
                Debug.LogError($"✗ Dynamic UI filtering test failed");
            }
        }
        
        /// <summary>
        /// Create sample test boards for testing
        /// </summary>
        [ContextMenu("Create Sample Test Boards")]
        public void CreateSampleTestBoards()
        {
            testBoards.Clear();
            
            // Create sample board data (this would normally be done in the editor)
            Debug.Log("Sample test boards created. Assign actual BoardData assets in the inspector for proper testing.");
            Debug.Log("You can create BoardData assets by right-clicking in the Project window and selecting 'Passive Tree > Board Data'");
        }
        
        /// <summary>
        /// Clear all selections and reset tracker
        /// </summary>
        [ContextMenu("Clear All Selections")]
        public void ClearAllSelections()
        {
            var tracker = BoardSelectionTracker.Instance;
            if (tracker != null)
            {
                tracker.ClearAllSelections();
                Debug.Log("All board selections cleared.");
            }
        }
        
        /// <summary>
        /// Print current selection statistics
        /// </summary>
        [ContextMenu("Print Selection Statistics")]
        public void PrintSelectionStatistics()
        {
            var tracker = BoardSelectionTracker.Instance;
            if (tracker != null)
            {
                var stats = tracker.GetSelectionStatistics();
                Debug.Log($"Selection Statistics:");
                Debug.Log($"  Total Selected Boards: {stats.TotalSelectedBoards}");
                Debug.Log($"  Total Tiers: {stats.TotalTiers}");
                Debug.Log($"  Total Themes: {stats.TotalThemes}");
                
                foreach (var tierPair in stats.SelectedBoardsByTier)
                {
                    Debug.Log($"  Tier {tierPair.Key}: {tierPair.Value} boards");
                }
            }
        }
    }
}
