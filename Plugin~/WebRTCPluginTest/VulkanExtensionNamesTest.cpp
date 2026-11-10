#include "pch.h"

#if SUPPORT_VULKAN

#include <string>

#include "GraphicsDevice/Vulkan/UnityVulkanInitCallback.h"

namespace unity
{
namespace webrtc
{
    TEST(VulkanExtensionNamesTest, ComparesNamesByValueAndPreservesUnityOrder)
    {
        std::string surface = VK_KHR_SURFACE_EXTENSION_NAME;
        std::string properties2 = VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME;
        const char* enabled[] = { surface.c_str(), properties2.c_str() };
        std::vector<const char*> requested = { VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME,
                                               VK_KHR_EXTERNAL_MEMORY_CAPABILITIES_EXTENSION_NAME,
                                               VK_KHR_EXTERNAL_MEMORY_CAPABILITIES_EXTENSION_NAME };

        auto names = MergeExtensionNames(enabled, 2, requested);

        ASSERT_EQ(names.size(), 3);
        EXPECT_EQ(names[0], enabled[0]);
        EXPECT_EQ(names[1], enabled[1]);
        EXPECT_STREQ(names[2], VK_KHR_EXTERNAL_MEMORY_CAPABILITIES_EXTENSION_NAME);
    }

    TEST(VulkanExtensionNamesTest, HandlesNoEnabledNames)
    {
        std::vector<const char*> requested = { VK_KHR_EXTERNAL_MEMORY_CAPABILITIES_EXTENSION_NAME };
        EXPECT_EQ(MergeExtensionNames(nullptr, 0, requested), requested);
    }

    TEST(VulkanExtensionNamesTest, HandlesNoRequestedNames)
    {
        const char* enabled[] = { VK_KHR_SURFACE_EXTENSION_NAME };
        auto names = MergeExtensionNames(enabled, 1, {});
        ASSERT_EQ(names.size(), 1);
        EXPECT_EQ(names[0], enabled[0]);
        EXPECT_TRUE(MergeExtensionNames(nullptr, 0, {}).empty());
    }
} // namespace webrtc
} // namespace unity

#endif
